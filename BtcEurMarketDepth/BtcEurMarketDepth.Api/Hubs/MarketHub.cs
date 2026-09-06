using Microsoft.AspNetCore.SignalR;

namespace BtcEurMarketDepth.Api.Hubs
{
    /// <summary>
    /// SignalR hub used by clients to receive live market-data updates.
    /// </summary>
    public class MarketHub : Hub
    {
    }
}
