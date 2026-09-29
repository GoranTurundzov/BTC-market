using System.Text.Json.Serialization;

namespace BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp
{
    /// <summary>
    /// Represents the order-book payload published by Bitstamp's WebSocket stream.
    /// </summary>
    public class BitstampOrderBookResponse
    {
        /// <summary>
        /// Gets the Unix timestamp returned by Bitstamp.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; } = string.Empty;

        [JsonPropertyName("microtimestamp")]
        public string Microtimestamp { get; init; } = string.Empty;

        /// <summary>
        /// Gets the bid levels returned by Bitstamp.
        /// Each level contains the price and quantity as strings.
        /// </summary>
        [JsonPropertyName("bids")]
        public List<string[]> Bids { get; init; } = [];

        /// <summary>
        /// Gets the ask levels returned by Bitstamp.
        /// Each level contains the price and quantity as strings.
        /// </summary>
        [JsonPropertyName("asks")]
        public List<string[]> Asks { get; init; } = [];
    }
}
