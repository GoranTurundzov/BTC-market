namespace BtcEurMarketDepth.Infrastructure.MarketData
{
        public class BitstampOrderBookResponse
        {
            public string Timestamp { get; init; } = string.Empty;

            public List<string[]> Bids { get; init; } = [];

            public List<string[]> Asks { get; init; } = [];
        }
    
}
