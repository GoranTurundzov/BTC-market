using BtcEurMarketDepth.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSource
    {
        Task<OrderBookSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default);
    }
}
