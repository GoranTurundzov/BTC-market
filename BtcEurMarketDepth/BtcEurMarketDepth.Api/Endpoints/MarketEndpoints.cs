using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;

namespace BtcEurMarketDepth.Api.Endpoints
{
    /// <summary>
    /// Defines HTTP endpoints for retrieving market data and purchase quotes.
    /// </summary>
    public static class MarketEndpoints
    {
        /// <summary>
        /// Registers the market-related API endpoints.
        /// </summary>
        /// <param name="endpoints">
        /// The endpoint route builder used to register the routes.
        /// </param>
        /// <returns>
        /// The same route builder so additional endpoint mappings can be chained.
        /// </returns>
        public static IEndpointRouteBuilder MapMarketEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/market/order-book", GetOrderBook);
            endpoints.MapGet("/api/market/quote", GetBuyQuote);

            return endpoints;
        }

        /// <summary>
        /// Returns the latest order-book snapshot currently held in memory.
        /// </summary>
        /// <param name="orderBookStore">
        /// Store containing the most recently acquired order book.
        /// </param>
        /// <returns>
        /// The latest order book, or HTTP 503 when no snapshot is available yet.
        /// </returns>
        private static IResult GetOrderBook(IOrderBookStore orderBookStore)
        {
            var orderBook = orderBookStore.GetLatest();

            return orderBook is null
                ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
                : Results.Ok(orderBook);
        }

        /// <summary>
        /// Calculates the cost of buying the requested amount of BTC
        /// using the current ask levels.
        /// </summary>
        /// <param name="quantity">
        /// The amount of BTC the user wants to buy.
        /// </param>
        /// <param name="orderBookStore">
        /// Store containing the latest order-book snapshot.
        /// </param>
        /// <param name="buyQuoteCalculator">
        /// Service used to calculate the purchase quote.
        /// </param>
        /// <returns>
        /// The calculated quote, or an appropriate HTTP error response when
        /// the quantity is invalid or no order book is available.
        /// </returns>
        private static IResult GetBuyQuote(
            decimal quantity,
            IOrderBookStore orderBookStore,
            IBuyQuoteCalculator buyQuoteCalculator)
        {
            if (quantity <= 0)
            {
                return Results.BadRequest(new
                {
                    error = "Quantity must be greater than zero."
                });
            }

            var orderBook = orderBookStore.GetLatest();

            if (orderBook is null)
            {
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var quote = buyQuoteCalculator.Calculate(orderBook, quantity);

            return Results.Ok(quote);
        }
    }
}
