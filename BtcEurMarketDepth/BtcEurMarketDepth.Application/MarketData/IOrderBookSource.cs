using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSource
    {
        IAsyncEnumerable<OrderBookSnapshot> ReadSnapshotsAsync(CancellationToken cancellationToken = default);
    }
}
