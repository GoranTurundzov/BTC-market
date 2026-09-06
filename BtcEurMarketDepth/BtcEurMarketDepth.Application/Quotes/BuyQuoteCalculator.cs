using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.Quotes
{
    public class BuyQuoteCalculator : IBuyQuoteCalculator
    {
        public BuyQuote Calculate(OrderBookSnapshot snapshot, decimal requestedQuantity)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            if (requestedQuantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedQuantity), "Requested quantity must be greater than zero.");
            }

            var remainingQuantity = requestedQuantity;
            var filledQuantity = 0m;
            var totalCost = 0m;

            foreach (var ask in snapshot.Asks)
            {
                if (remainingQuantity <= 0)
                {
                    break;
                }

                var quantityAtLevel = Math.Min(remainingQuantity, ask.Quantity);

                filledQuantity += quantityAtLevel;
                totalCost += quantityAtLevel * ask.Price;
                remainingQuantity -= quantityAtLevel;
            }

            decimal? averagePrice = filledQuantity > 0 ? totalCost / filledQuantity : null;

            decimal? bestAsk = snapshot.Asks.Count > 0 ? snapshot.Asks[0].Price : null;

            return new BuyQuote(
                requestedQuantity: requestedQuantity,
                filledQuantity: filledQuantity,
                remainingQuantity: remainingQuantity,
                totalCost: totalCost,
                averagePrice: averagePrice,
                bestAsk: bestAsk,
                isFullyFillable: remainingQuantity == 0,
                snapshotTime: snapshot.AcquiredAt,
                snapshotSequence: snapshot.Sequence);
        }
    }
}
