using System.Text.Json.Serialization;

namespace BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp
{
    internal sealed class BitstampWebSocketMessage
    {
        [JsonPropertyName("channel")]
        public string Channel { get; init; } = string.Empty;

        [JsonPropertyName("data")]
        public BitstampOrderBookResponse? Data { get; init; }

        [JsonPropertyName("event")]
        public string Event { get; init; } = string.Empty;

        [JsonPropertyName("event_id")]
        public string EventId { get; init; } = string.Empty;

        [JsonPropertyName("pre_event_id")]
        public string PreEventId { get; init; } = string.Empty;
    }
}
