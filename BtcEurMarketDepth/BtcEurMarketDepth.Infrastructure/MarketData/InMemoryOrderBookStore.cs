using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    public class InMemoryOrderBookStore : IOrderBookStore
    {
        private OrderBookSnapshot? latestSnapshot;

        public OrderBookSnapshot? GetLatest()
        {
            return Volatile.Read(ref latestSnapshot);
        }

        public void SetLatest(OrderBookSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            Interlocked.Exchange(ref latestSnapshot, snapshot);
        }
    }
}
