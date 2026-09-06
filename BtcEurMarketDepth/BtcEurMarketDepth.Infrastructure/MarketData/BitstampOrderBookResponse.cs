namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    /// <summary>
    /// Represents the JSON response returned by Bitstamp's order-book endpoint.
    /// </summary>
    public class BitstampOrderBookResponse
    {
        /// <summary>
        /// Gets the Unix timestamp returned by Bitstamp.
        /// </summary>
        public string Timestamp { get; init; } = string.Empty;

        /// <summary>
        /// Gets the bid levels returned by Bitstamp.
        /// Each level contains the price and quantity as strings.
        /// </summary>
        public List<string[]> Bids { get; init; } = [];

        /// <summary>
        /// Gets the ask levels returned by Bitstamp.
        /// Each level contains the price and quantity as strings.
        /// </summary>
        public List<string[]> Asks { get; init; } = [];
    }
}
