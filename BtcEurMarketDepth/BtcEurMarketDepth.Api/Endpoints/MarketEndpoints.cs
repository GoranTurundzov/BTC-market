using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;

namespace BtcEurMarketDepth.Api.Endpoints
{
    public static class MarketEndpoints
    {
        public static IEndpointRouteBuilder MapMarketEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/market/order-book", GetOrderBook);
            endpoints.MapGet("/api/market/quote", GetBuyQuote);

            return endpoints;
        }

        private static IResult GetOrderBook(IOrderBookStore orderBookStore)
        {
            var orderBook = orderBookStore.GetLatest();

            return orderBook is null
                ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
                : Results.Ok(orderBook);
        }

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
