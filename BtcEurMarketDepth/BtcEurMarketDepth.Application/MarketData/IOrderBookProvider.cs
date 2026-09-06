using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookProvider
    {
        Task<OrderBookSnapshot> GetLatestAsync(CancellationToken cancellationToken = default);
    }
}
