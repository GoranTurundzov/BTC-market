using BtcEurMarketDepth.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookUpdateNotifier
    {
        Task NotifyAsync(OrderBookSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
