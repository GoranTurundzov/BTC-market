using BtcEurMarketDepth.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.Quotes
{
    public interface IBuyQuoteCalculator
    {
        BuyQuote Calculate(OrderBookSnapshot snapshot, decimal requestedQuantity);
    }
}
