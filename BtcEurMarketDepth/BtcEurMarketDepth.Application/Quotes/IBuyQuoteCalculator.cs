using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.Quotes
{
    public interface IBuyQuoteCalculator
    {
        BuyQuote Calculate(OrderBookSnapshot snapshot, decimal requestedQuantity);
    }
}
