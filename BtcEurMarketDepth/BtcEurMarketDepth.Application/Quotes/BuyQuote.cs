using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.Quotes
{
    public record BuyQuote
    {
        public decimal RequestedQuantity { get; }
        public decimal FilledQuantity { get; }
        public decimal RemainingQuantity { get; }
        public decimal TotalCost { get; }
        public decimal? AveragePrice { get; }
        public decimal? BestAsk { get; }
        public bool IsFullyFillable { get; }
        public DateTimeOffset SnapshotTime { get; }
        public long SnapshotSequence { get; }

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
