using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    public class BitstampOrderBookSource(HttpClient httpClient) : IOrderBookSource
    {
        private const string MarketSymbol = "btceur";
        private const string OrderBookEndpoint = $"/api/v2/order_book/{MarketSymbol}/";

        public async Task<OrderBookSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(OrderBookEndpoint, cancellationToken);

            response.EnsureSuccessStatusCode();

            var orderBook = await response.Content.ReadFromJsonAsync<BitstampOrderBookResponse>(
                cancellationToken);

            return CreateSnapshot(orderBook ?? throw new InvalidOperationException("Bitstamp returned an empty order-book response."));
        }

        private static OrderBookSnapshot CreateSnapshot(BitstampOrderBookResponse response)
        {
            var bids = ParseLevels(response.Bids)
                .OrderByDescending(level => level.Price)
                .ToArray();

            var asks = ParseLevels(response.Asks)
                .OrderBy(level => level.Price)
                .ToArray();

            return new OrderBookSnapshot(
                symbol: "BTC/EUR",
                acquiredAt: ParseTimestamp(response.Timestamp),
                bids: bids,
                asks: asks,
                sequence: 0);
        }

        private static IEnumerable<PriceLevel> ParseLevels(IReadOnlyList<string[]> levels)
        {
            foreach (var level in levels)
            {
                if (level.Length != 2)
                {
                    throw new JsonException("Bitstamp returned an invalid price level.");
                }

                if (!decimal.TryParse(
                        level[0],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    throw new JsonException($"Invalid price returned by Bitstamp: {level[0]}");
                }

                if (!decimal.TryParse(
                        level[1],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var quantity))
                {
                    throw new JsonException($"Invalid quantity returned by Bitstamp: {level[1]}");
                }

                yield return new PriceLevel(price, quantity);
            }
        }

        private static DateTimeOffset ParseTimestamp(string timestamp)
        {
            if (long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }

            return DateTimeOffset.UtcNow;
        }
    }
}
