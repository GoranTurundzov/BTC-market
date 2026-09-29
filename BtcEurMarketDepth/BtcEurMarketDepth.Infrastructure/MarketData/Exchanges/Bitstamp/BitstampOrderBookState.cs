using System.Globalization;
using System.Text.Json;

using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp
{
    /// <summary>
    /// Maintains the complete reconstructed Bitstamp order book.
    /// </summary>
    internal sealed class BitstampOrderBookState
    {
        private readonly Dictionary<decimal, decimal> bids = [];
        private readonly Dictionary<decimal, decimal> asks = [];

        public void Load(BitstampOrderBookResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            bids.Clear();
            asks.Clear();

            ApplyLevels(response.Bids, bids);
            ApplyLevels(response.Asks, asks);
        }

        public void Apply(BitstampOrderBookResponse update)
        {
            ArgumentNullException.ThrowIfNull(update);

            ApplyLevels(update.Bids, bids);
            ApplyLevels(update.Asks, asks);
        }

        public OrderBookSnapshot CreateSnapshot(
            string symbol,
            DateTimeOffset acquiredAt,
            long sequence)
        {
            return new OrderBookSnapshot(
                symbol,
                acquiredAt,
                bids
                    .OrderByDescending(level => level.Key)
                    .Select(level => new PriceLevel(level.Key, level.Value)),
                asks
                    .OrderBy(level => level.Key)
                    .Select(level => new PriceLevel(level.Key, level.Value)),
                sequence);
        }

        private static void ApplyLevels(
            IReadOnlyList<string[]> levels,
            IDictionary<decimal, decimal> target)
        {
            foreach (var level in levels)
            {
                if (level.Length != 2)
                {
                    throw new JsonException(
                        "Bitstamp returned an invalid price level.");
                }

                if (!decimal.TryParse(
                        level[0],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    throw new JsonException(
                        $"Invalid price returned by Bitstamp: {level[0]}");
                }

                if (!decimal.TryParse(
                        level[1],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var quantity))
                {
                    throw new JsonException(
                        $"Invalid quantity returned by Bitstamp: {level[1]}");
                }

                if (quantity == 0m)
                {
                    target.Remove(price);
                }
                else if (quantity > 0m)
                {
                    target[price] = quantity;
                }
                else
                {
                    throw new JsonException(
                        "Bitstamp returned a negative quantity.");
                }
            }
        }
    }
}
