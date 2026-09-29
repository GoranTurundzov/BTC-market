using System.Globalization;
using System.Net.WebSockets;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;
using BtcEurMarketDepth.Infrastructure.Configuration;
using BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp
{
    /// <summary>
    /// Reconstructs the full Bitstamp order book from a REST bootstrap and
    /// incremental WebSocket updates.
    /// </summary>
    public sealed class BitstampOrderBookSource(
        ILogger<BitstampOrderBookSource> logger,
        IOptions<BitstampWebSocketOptions> options,
        IClientWebSocketFactory socketFactory,
        HttpClient httpClient) : IOrderBookSource
    {
        private readonly BitstampWebSocketOptions settings = options.Value;

        public async IAsyncEnumerable<OrderBookSnapshot> ReadSnapshotsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await using var enumerator = ReadConnectionAsync(cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    bool hasSnapshot;

                    try
                    {
                        hasSnapshot = await enumerator.MoveNextAsync();
                    }
                    catch (OperationCanceledException)
                        when (cancellationToken.IsCancellationRequested)
                    {
                        yield break;
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning(
                            exception,
                            "Bitstamp order-book connection failed. " +
                            "Reconnecting in {ReconnectDelaySeconds} seconds.",
                            settings.ReconnectDelaySeconds);

                        break;
                    }

                    if (!hasSnapshot)
                    {
                        break;
                    }

                    yield return enumerator.Current;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(settings.ReconnectDelaySeconds),
                    cancellationToken);
            }
        }

        private async IAsyncEnumerable<OrderBookSnapshot> ReadConnectionAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var socket = socketFactory.Create();

            await socket.ConnectAsync(
                new Uri(settings.Url),
                cancellationToken);

            var expectedChannel =
                $"diff_order_book_{settings.MarketSymbol}";

            await SendSubscriptionAsync(
                socket,
                expectedChannel,
                cancellationToken);

            var state = new BitstampOrderBookState();
            var pendingUpdates = new List<BitstampWebSocketMessage>();
            var bootstrapTask = LoadRestSnapshotAsync(cancellationToken);
            var receiveTask = ReceiveMessageAsync(socket, cancellationToken);

            while (!bootstrapTask.IsCompleted)
            {
                var completedTask = await Task.WhenAny(
                    receiveTask,
                    bootstrapTask);

                if (completedTask == bootstrapTask)
                {
                    break;
                }

                var message = await receiveTask;
                if (message is null)
                {
                    yield break;
                }

                receiveTask = ReceiveMessageAsync(socket, cancellationToken);

                var envelope = DeserializeDataMessage(message, expectedChannel);
                if (envelope is not null)
                {
                    pendingUpdates.Add(envelope);
                }
            }

            var restSnapshot = await bootstrapTask;
            state.Load(restSnapshot);

            var lastEventId = string.Empty;
            var sequence = 0L;

            yield return state.CreateSnapshot(
                settings.DisplaySymbol,
                ParseTimestamp(restSnapshot.Timestamp, restSnapshot.Microtimestamp),
                sequence++);

            foreach (var update in pendingUpdates)
            {
                if (!IsAfterSnapshot(update.Data!, restSnapshot))
                {
                    continue;
                }

                ApplyUpdate(update, state, ref lastEventId);

                yield return state.CreateSnapshot(
                    settings.DisplaySymbol,
                    ParseTimestamp(update.Data.Timestamp, update.Data.Microtimestamp),
                    sequence++);
            }

            while (socket.State == WebSocketState.Open)
            {
                var message = await receiveTask;
                if (message is null)
                {
                    yield break;
                }

                receiveTask = ReceiveMessageAsync(socket, cancellationToken);

                var envelope = DeserializeDataMessage(message, expectedChannel);
                if (envelope is null)
                {
                    continue;
                }

                ApplyUpdate(envelope, state, ref lastEventId);

                yield return state.CreateSnapshot(
                    settings.DisplaySymbol,
                    ParseTimestamp(envelope.Data.Timestamp, envelope.Data.Microtimestamp),
                    sequence++);
            }
        }

        private async Task<BitstampOrderBookResponse> LoadRestSnapshotAsync(
            CancellationToken cancellationToken)
        {
            var requestPath =
                $"order_book/{settings.MarketSymbol}/?group=1";

            using var response = await httpClient.GetAsync(
                requestPath,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BitstampOrderBookResponse>(
                       cancellationToken)
                   ?? throw new InvalidOperationException(
                       "Bitstamp returned an empty order-book response.");
        }

        private static async Task SendSubscriptionAsync(
            IWebSocketClient socket,
            string channel,
            CancellationToken cancellationToken)
        {
            var subscription = JsonSerializer.Serialize(new
            {
                @event = "bts:subscribe",
                data = new
                {
                    channel
                }
            });

            await socket.SendAsync(
                Encoding.UTF8.GetBytes(subscription),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }

        private static BitstampWebSocketMessage? DeserializeDataMessage(
            string message,
            string expectedChannel)
        {
            var envelope = JsonSerializer.Deserialize<BitstampWebSocketMessage>(message);

            if (envelope?.Event is "error" or "bts:request_reconnect")
            {
                throw new InvalidOperationException(
                    $"Bitstamp requested a reconnect or returned an error event: {envelope.Event}.");
            }

            if (envelope is null ||
                envelope.Event != "data" ||
                envelope.Channel != expectedChannel ||
                envelope.Data is null)
            {
                return null;
            }

            return envelope;
        }

        private static void ApplyUpdate(
            BitstampWebSocketMessage update,
            BitstampOrderBookState state,
            ref string lastEventId)
        {
            if (!string.IsNullOrWhiteSpace(lastEventId) &&
                !string.IsNullOrWhiteSpace(update.PreEventId) &&
                !string.Equals(
                    update.PreEventId,
                    lastEventId,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Bitstamp order-book event sequence contains a gap.");
            }

            state.Apply(update.Data!);

            if (!string.IsNullOrWhiteSpace(update.EventId))
            {
                lastEventId = update.EventId;
            }
        }

        private static bool IsAfterSnapshot(
            BitstampOrderBookResponse update,
            BitstampOrderBookResponse snapshot)
        {
            if (!long.TryParse(
                    update.Microtimestamp,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var updateTime) ||
                !long.TryParse(
                    snapshot.Microtimestamp,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var snapshotTime))
            {
                return true;
            }

            return updateTime > snapshotTime;
        }

        private static DateTimeOffset ParseTimestamp(
            string timestamp,
            string microtimestamp)
        {
            if (long.TryParse(
                    microtimestamp,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var microseconds))
            {
                return DateTimeOffset.UnixEpoch.AddTicks(microseconds * 10);
            }

            if (long.TryParse(
                    timestamp,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds);
            }

            return DateTimeOffset.UtcNow;
        }

        private static async Task<string?> ReceiveMessageAsync(
            IWebSocketClient socket,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[8192];
            using var message = new MemoryStream();

            while (true)
            {
                var result = await socket.ReceiveAsync(
                    buffer,
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                message.Write(buffer, 0, result.Count);

                if (result.EndOfMessage)
                {
                    return Encoding.UTF8.GetString(message.ToArray());
                }
            }
        }
    }
}
