namespace BtcEurMarketDepth.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration for the Bitstamp order-book feed and publication cadence.
    /// </summary>
    public sealed class BitstampWebSocketOptions
    {
        public const string SectionName = "BitstampWebSocket";

        public string Url { get; init; } = string.Empty;

        public string RestApiUrl { get; init; } = string.Empty;

        public string MarketSymbol { get; init; } = string.Empty;

        public string DisplaySymbol { get; init; } = string.Empty;

        public int ReconnectDelaySeconds { get; init; } = 5;

        public int PublishIntervalMilliseconds { get; init; } = 1000;
    }
}
