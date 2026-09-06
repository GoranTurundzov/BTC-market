using BtcEurMarketDepth.Api.Hubs;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

using Microsoft.AspNetCore.SignalR;

namespace BtcEurMarketDepth.Api.MarketData
{
    public sealed class SignalROrderBookUpdateNotifier(
    IHubContext<MarketHub> hubContext) : IOrderBookUpdateNotifier
    {
        public Task NotifyAsync(
            OrderBookSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            return hubContext.Clients.All.SendAsync("OrderBookUpdated", snapshot, cancellationToken);
        }
    }
}
