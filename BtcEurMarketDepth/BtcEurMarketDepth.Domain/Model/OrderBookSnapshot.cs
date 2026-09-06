using System.Collections.ObjectModel;

namespace BtcEurMarketDepth.Domain.Model
{
    public record OrderBookSnapshot
    {
        public string Symbol { get; }
        public DateTimeOffset AcquiredAt { get; }
        public IReadOnlyList<PriceLevel> Bids { get; }
        public IReadOnlyList<PriceLevel> Asks { get; }
        public long Sequence { get; }

        public OrderBookSnapshot(
            string symbol,
            DateTimeOffset acquiredAt,
            IEnumerable<PriceLevel> bids,
            IEnumerable<PriceLevel> asks,
            long sequence)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                throw new ArgumentException("Symbol is required.", nameof(symbol));
            }

            ArgumentNullException.ThrowIfNull(bids);
            ArgumentNullException.ThrowIfNull(asks);

            Symbol = symbol.Trim();
            AcquiredAt = acquiredAt;
            Sequence = sequence;

            var bidLevels = bids.ToArray();
            var askLevels = asks.ToArray();

            ValidateBidLevels(bidLevels);
            ValidateAskLevels(askLevels);

            Bids = new ReadOnlyCollection<PriceLevel>(bidLevels);
            Asks = new ReadOnlyCollection<PriceLevel>(askLevels);
        }

        private static void ValidateBidLevels(PriceLevel[] bids)
        {
            ValidateUniquePrices(bids, "Bids");

            for (var index = 1; index < bids.Length; index++)
            {
                if (bids[index - 1].Price < bids[index].Price)
                {
                    throw new ArgumentException("Bids must be ordered by price descending.", nameof(bids));
                }
            }
        }

        private static void ValidateAskLevels(PriceLevel[] asks)
        {
            ValidateUniquePrices(asks, "Asks");

            for (var index = 1; index < asks.Length; index++)
            {
                if (asks[index - 1].Price > asks[index].Price)
                {
                    throw new ArgumentException("Asks must be ordered by price ascending.", nameof(asks));
                }
            }
        }

        private static void ValidateUniquePrices(IReadOnlyList<PriceLevel> levels, string side)
        {
            var prices = new HashSet<decimal>();

            foreach (var level in levels)
            {
                if (!prices.Add(level.Price))
                {
                    throw new ArgumentException($"{side} cannot contain duplicate price levels.", nameof(levels));
                }
            }
        }
    }
}
