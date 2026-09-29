using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;
using BtcEurMarketDepth.Infrastructure.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Threading.Channels;

namespace BtcEurMarketDepth.Infrastructure.MarketData.Processing
{
    /// <summary>
    /// Consumes live order-book snapshots and sends each one through the audit,
    /// latest-state, and client-notification pipeline.
    /// </summary>
    public sealed class OrderBookStreamingService(
        IOrderBookSource orderBookSource,
        IOrderBookStore orderBookStore,
        IOrderBookUpdateNotifier orderBookUpdateNotifier,
        IOrderBookSnapshotAuditRepository auditRepository,
        ILogger<OrderBookStreamingService> logger,
        IOptions<BitstampWebSocketOptions> options) : BackgroundService
    {
        private readonly TimeSpan publishInterval =
            TimeSpan.FromMilliseconds(options.Value.PublishIntervalMilliseconds);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channel = Channel.CreateBounded<OrderBookSnapshot>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true,
                    SingleWriter = true
                });

            var receiverTask = ReceiveSnapshotsAsync(
                channel.Writer,
                stoppingToken);

            var publisherTask = PublishSnapshotsAsync(
                channel.Reader,
                stoppingToken);

            await Task.WhenAll(receiverTask, publisherTask);
        }

        private async Task ReceiveSnapshotsAsync(
            ChannelWriter<OrderBookSnapshot> writer,
            CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var snapshot in orderBookSource
                    .ReadSnapshotsAsync(cancellationToken)
                    .WithCancellation(cancellationToken))
                {
                    await writer.WriteAsync(snapshot, cancellationToken);
                }
            }
            finally
            {
                writer.TryComplete();
            }
        }

        private async Task PublishSnapshotsAsync(
            ChannelReader<OrderBookSnapshot> reader,
            CancellationToken cancellationToken)
        {
            while (await reader.WaitToReadAsync(cancellationToken))
            {
                OrderBookSnapshot? newestSnapshot = null;

                while (reader.TryRead(out var snapshot))
                {
                    newestSnapshot = snapshot;
                }

                if (newestSnapshot is not null)
                {
                    await ProcessSnapshotAsync(
                        newestSnapshot,
                        cancellationToken);
                }

                await Task.Delay(
                    publishInterval,
                    cancellationToken);
            }
        }

        private async Task ProcessSnapshotAsync(
            OrderBookSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            try
            {
                await auditRepository.SaveAsync(snapshot, cancellationToken);

                orderBookStore.SetLatest(snapshot);

                await orderBookUpdateNotifier.NotifyAsync(snapshot, cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Updated {Symbol} order book with {BidCount} bids and {AskCount} asks at {AcquiredAt}.",
                        snapshot.Symbol,
                        snapshot.Bids.Count,
                        snapshot.Asks.Count,
                        snapshot.AcquiredAt);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to audit and publish the latest order-book snapshot.");
            }
        }
    }
}
