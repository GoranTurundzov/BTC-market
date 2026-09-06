using System.Collections.ObjectModel;

namespace BtcEurMarketDepth.Domain.Model
{
    /// <summary>
    /// Represents one acquired snapshot of the order book at a specific point in time.
    /// </summary>
    public record OrderBookSnapshot
    {
        /// <summary>
        /// Gets the market symbol represented by the snapshot.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Gets the time when the snapshot was acquired from the exchange.
        /// </summary>
        public DateTimeOffset AcquiredAt { get; }

        /// <summary>
        /// Gets the bid levels ordered from the highest price to the lowest price.
        /// </summary>
        public IReadOnlyList<PriceLevel> Bids { get; }

        /// <summary>
        /// Gets the ask levels ordered from the lowest price to the highest price.
        /// </summary>
        public IReadOnlyList<PriceLevel> Asks { get; }

        /// <summary>
        /// Gets the exchange-provided sequence number associated with the snapshot.
        /// </summary>
        public long Sequence { get; }

        /// <summary>
        /// Creates a validated and immutable order-book snapshot.
        /// </summary>
        /// <param name="symbol">The market symbol.</param>
        /// <param name="acquiredAt">The time when the snapshot was acquired.</param>
        /// <param name="bids">Bid levels ordered by descending price.</param>
        /// <param name="asks">Ask levels ordered by ascending price.</param>
        /// <param name="sequence">The snapshot sequence number.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the symbol is empty, or when the levels are incorrectly
        /// ordered or contain duplicate prices.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when bids or asks are null.
        /// </exception>
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

        /// <summary>
        /// Verifies that bids contain unique prices and are ordered from highest
        /// to lowest price.
        /// </summary>
        /// <param name="bids">The bid levels to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when duplicate prices exist or the ordering is invalid.
        /// </exception>
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

        /// <summary>
        /// Verifies that asks contain unique prices and are ordered from lowest
        /// to highest price.
        /// </summary>
        /// <param name="asks">The ask levels to validate.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when duplicate prices exist or the ordering is invalid.
        /// </exception>
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

        /// <summary>
        /// Verifies that no two levels use the same price.
        /// </summary>
        /// <param name="levels">The price levels to validate.</param>
        /// <param name="side">The order-book side being validated.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when duplicate prices are found.
        /// </exception>
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
