using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    /// <summary>
    /// Converts the raw Bitstamp response into a validated domain snapshot.
    /// </summary>
    /// <param name="response">
    /// The response received from Bitstamp.
    /// </param>
    /// <returns>
    /// A snapshot with bids sorted by descending price and asks sorted by ascending price.
    /// </returns>
    public class BitstampOrderBookSource(HttpClient httpClient) : IOrderBookSource
    {
        private const string MarketSymbol = "btceur";
        private const string OrderBookEndpoint = $"/api/v2/order_book/{MarketSymbol}/";

        /// <summary>
        /// Retrieves the latest BTC/EUR order book from Bitstamp and converts it
        /// into the domain snapshot used by the application.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the HTTP request.
        /// </param>
        /// <returns>
        /// A normalized order-book snapshot with bids ordered from highest to lowest
        /// price and asks ordered from lowest to highest price.
        /// </returns>
        /// <exception cref="HttpRequestException">
        /// Thrown when Bitstamp returns an unsuccessful response.
        /// </exception>
        public async Task<OrderBookSnapshot> FetchLatestAsync(CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.GetAsync(OrderBookEndpoint, cancellationToken);

            response.EnsureSuccessStatusCode();

            var orderBook = await response.Content.ReadFromJsonAsync<BitstampOrderBookResponse>(
                cancellationToken);

            return CreateSnapshot(orderBook ?? throw new InvalidOperationException("Bitstamp returned an empty order-book response."));
        }

        private static OrderBookSnapshot CreateSnapshot(BitstampOrderBookResponse response)
        {
            var bids = ParseLevels(response.Bids)
                .OrderByDescending(level => level.Price)
                .ToArray();

            var asks = ParseLevels(response.Asks)
                .OrderBy(level => level.Price)
                .ToArray();

            return new OrderBookSnapshot(
                symbol: "BTC/EUR",
                acquiredAt: ParseTimestamp(response.Timestamp),
                bids: bids,
                asks: asks,
                sequence: 0);
        }

        /// <summary>
        /// Parses Bitstamp's string-based price levels into domain price levels.
        /// </summary>
        /// <param name="levels">
        /// Raw price and quantity pairs returned by Bitstamp.
        /// </param>
        /// <returns>
        /// The parsed price levels.
        /// </returns>
        /// <exception cref="JsonException">
        /// Thrown when a level does not contain exactly two values or when a value
        /// cannot be parsed as a decimal.
        /// </exception>
        private static IEnumerable<PriceLevel> ParseLevels(IReadOnlyList<string[]> levels)
        {
            foreach (var level in levels)
            {
                if (level.Length != 2)
                {
                    throw new JsonException("Bitstamp returned an invalid price level.");
                }

                if (!decimal.TryParse(
                        level[0],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    throw new JsonException($"Invalid price returned by Bitstamp: {level[0]}");
                }

                if (!decimal.TryParse(
                        level[1],
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var quantity))
                {
                    throw new JsonException($"Invalid quantity returned by Bitstamp: {level[1]}");
                }

                yield return new PriceLevel(price, quantity);
            }
        }

        /// <summary>
        /// Converts Bitstamp's Unix timestamp into a date and time.
        /// </summary>
        /// <param name="timestamp">
        /// The Unix timestamp represented as text.
        /// </param>
        /// <returns>
        /// The parsed timestamp, or the current UTC time when the value is invalid.
        /// </returns>
        private static DateTimeOffset ParseTimestamp(string timestamp)
        {
            if (long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            }

            return DateTimeOffset.UtcNow;
        }
    }
}
