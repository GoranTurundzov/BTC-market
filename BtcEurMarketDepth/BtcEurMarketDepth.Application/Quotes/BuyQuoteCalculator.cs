using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Application.Quotes
{
    /// <summary>
    /// Calculates the estimated cost of buying BTC using the available ask levels.
    /// </summary>
    public class BuyQuoteCalculator : IBuyQuoteCalculator
    {
        /// <summary>
        /// Calculates a purchase quote by consuming ask levels from the lowest
        /// available price upwards.
        /// </summary>
        /// <param name="snapshot">
        /// The order-book snapshot used for the calculation.
        /// </param>
        /// <param name="requestedQuantity">
        /// The amount of BTC the user wants to buy.
        /// </param>
        /// <returns>
        /// A quote containing the filled quantity, total cost, average price,
        /// remaining quantity, and snapshot information.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="snapshot"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the requested quantity is zero or negative.
        /// </exception>
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
