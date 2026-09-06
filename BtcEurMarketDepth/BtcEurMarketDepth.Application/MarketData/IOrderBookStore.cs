using BtcEurMarketDepth.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookStore
    {
        OrderBookSnapshot? GetLatest();

        void SetLatest(OrderBookSnapshot snapshot);
    }
}
