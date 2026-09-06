using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.Quotes
{
    /// <summary>
    /// Represents the estimated result of buying a requested amount of BTC
    /// from the current order book.
    /// </summary>
    public record BuyQuote
    {
        /// <summary>
        /// Gets the amount of BTC requested by the user.
        /// </summary>
        public decimal RequestedQuantity { get; }

        /// <summary>
        /// Gets the amount of BTC that can be filled using the available asks.
        /// </summary>
        public decimal FilledQuantity { get; }

        /// <summary>
        /// Gets the amount of BTC that cannot currently be filled.
        /// </summary>
        public decimal RemainingQuantity { get; }

        /// <summary>
        /// Gets the total estimated purchase cost in EUR.
        /// </summary>
        public decimal TotalCost { get; }

        /// <summary>
        /// Gets the weighted average execution price in EUR per BTC.
        /// </summary>
        public decimal? AveragePrice { get; }

        /// <summary>
        /// Gets the lowest available ask price at the time of the calculation.
        /// </summary>
        public decimal? BestAsk { get; }

        /// <summary>
        /// Gets a value indicating whether the complete requested quantity
        /// can be purchased.
        /// </summary>
        public bool IsFullyFillable { get; }

        /// <summary>
        /// Gets the time when the order-book snapshot was acquired.
        /// </summary>
        public DateTimeOffset SnapshotTime { get; }

        /// <summary>
        /// Gets the sequence number of the order-book snapshot used.
        /// </summary>
        public long SnapshotSequence { get; }

        /// <summary>
        /// Creates a new purchase quote.
        /// </summary>
        /// <param name="requestedQuantity">Requested BTC quantity.</param>
        /// <param name="filledQuantity">BTC quantity that can be filled.</param>
        /// <param name="remainingQuantity">BTC quantity that remains unfilled.</param>
        /// <param name="totalCost">Total estimated cost in EUR.</param>
        /// <param name="averagePrice">Weighted average execution price.</param>
        /// <param name="bestAsk">Lowest available ask price.</param>
        /// <param name="isFullyFillable">
        /// Whether the complete requested quantity can be filled.
        /// </param>
        /// <param name="snapshotTime">Snapshot acquisition time.</param>
        /// <param name="snapshotSequence">Snapshot sequence number.</param>
        public BuyQuote(
            decimal requestedQuantity,
            decimal filledQuantity,
            decimal remainingQuantity,
            decimal totalCost,
            decimal? averagePrice,
            decimal? bestAsk,
            bool isFullyFillable,
            DateTimeOffset snapshotTime,
            long snapshotSequence)
        {
            RequestedQuantity = requestedQuantity;
            FilledQuantity = filledQuantity;
            RemainingQuantity = remainingQuantity;
            TotalCost = totalCost;
            AveragePrice = averagePrice;
            BestAsk = bestAsk;
            IsFullyFillable = isFullyFillable;
            SnapshotTime = snapshotTime;
            SnapshotSequence = snapshotSequence;
        }
    }
}
