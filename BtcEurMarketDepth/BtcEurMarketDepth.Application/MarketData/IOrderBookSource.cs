using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSource
    {
        Task<OrderBookSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default);
    }
}
