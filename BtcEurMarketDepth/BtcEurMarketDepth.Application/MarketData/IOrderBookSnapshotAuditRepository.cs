using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSnapshotAuditRepository
    {
        Task SaveAsync(OrderBookSnapshot snapshot, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrderBookSnapshot>> GetHistoryAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default);

        Task<OrderBookSnapshot?> GetClosestAsync(
            DateTimeOffset timestamp,
            CancellationToken cancellationToken = default);
    }
}
