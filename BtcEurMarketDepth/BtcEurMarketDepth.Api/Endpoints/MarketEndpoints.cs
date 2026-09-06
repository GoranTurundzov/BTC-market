using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Api.Endpoints
{
    public static class MarketEndpoints
    {
        public static IEndpointRouteBuilder MapMarketEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/api/market/order-book", GetOrderBook);

            return endpoints;
        }

        private static OrderBookSnapshot GetOrderBook()
        {
            return new OrderBookSnapshot(
                symbol: "BTC/EUR",
                acquiredAt: DateTimeOffset.UtcNow,
                bids:
                [
                new PriceLevel(29_900m, 1.2m),
                new PriceLevel(29_800m, 2.5m),
                new PriceLevel(29_700m, 4.0m)
                ],
                asks:
                [
                new PriceLevel(30_000m, 1.0m),
                new PriceLevel(30_100m, 4.0m),
                new PriceLevel(30_200m, 9.0m)
                ],
                sequence: 1);
        }
    }
}
