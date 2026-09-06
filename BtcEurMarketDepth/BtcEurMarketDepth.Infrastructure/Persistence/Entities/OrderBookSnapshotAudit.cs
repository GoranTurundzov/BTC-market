namespace BtcEurMarketDepth.Infrastructure.Persistence.Entities
{
    public sealed class OrderBookSnapshotAudit
    {
        public long Id { get; set; }

        public string Symbol { get; set; } = string.Empty;

        public long Sequence { get; set; }

        public DateTimeOffset AcquiredAt { get; set; }

        public DateTimeOffset RecordedAt { get; set; }

        public decimal? BestBid { get; set; }

        public decimal? BestAsk { get; set; }

        public string BidsJson { get; set; } = string.Empty;

        public string AsksJson { get; set; } = string.Empty;
    }
}
