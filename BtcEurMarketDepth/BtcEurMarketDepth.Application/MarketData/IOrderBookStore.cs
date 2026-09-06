using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookStore
    {
        OrderBookSnapshot? GetLatest();

        void SetLatest(OrderBookSnapshot snapshot);
    }
}
