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

        private static async Task<IResult> GetOrderBook(
            IOrderBookProvider orderBookProvider,
            CancellationToken cancellationToken)
        {
            var orderBook = await orderBookProvider.GetLatestAsync(cancellationToken);

            return Results.Ok(orderBook);
        }

        private static async Task<IResult> GetBuyQuote(
            decimal quantity,
            IOrderBookProvider orderBookProvider,
            IBuyQuoteCalculator buyQuoteCalculator,
            CancellationToken cancellationToken)
        {
            if (quantity <= 0)
            {
                return Results.BadRequest(new
                {
                    error = "Quantity must be greater than zero."
                });
            }

            var orderBook = await orderBookProvider.GetLatestAsync(cancellationToken);
            var quote = buyQuoteCalculator.Calculate(orderBook, quantity);

            return Results.Ok(quote);
        }
    }
}
