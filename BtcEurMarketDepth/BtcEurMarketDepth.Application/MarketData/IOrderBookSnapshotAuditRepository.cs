using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSnapshotAuditRepository
    {
        Task SaveAsync(OrderBookSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
