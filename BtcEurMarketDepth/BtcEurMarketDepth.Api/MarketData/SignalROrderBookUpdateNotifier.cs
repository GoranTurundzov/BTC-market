using BtcEurMarketDepth.Api.Hubs;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

using Microsoft.AspNetCore.SignalR;

namespace BtcEurMarketDepth.Api.MarketData
{
    /// <summary>
    /// Publishes order-book updates to all connected frontend clients through SignalR.
    /// </summary>
    public sealed class SignalROrderBookUpdateNotifier(
    IHubContext<MarketHub> hubContext) : IOrderBookUpdateNotifier
    {
        /// <summary>
        /// Sends the latest order-book snapshot to every connected SignalR client.
        /// </summary>
        /// <param name="snapshot">
        /// The order-book snapshot to publish.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the notification.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous notification operation.
        /// </returns>
        public Task NotifyAsync(
            OrderBookSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            return hubContext.Clients.All.SendAsync("OrderBookUpdated", snapshot, cancellationToken);
        }
    }
}
