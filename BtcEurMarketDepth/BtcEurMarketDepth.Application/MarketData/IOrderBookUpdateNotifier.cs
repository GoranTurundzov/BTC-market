using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookUpdateNotifier
    {
        Task NotifyAsync(OrderBookSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
