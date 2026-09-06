using System.Net;
using System.Text;
using System.Text.Json;

using BtcEurMarketDepth.Infrastructure.MarketData;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class BitstampOrderBookSourceTests
    {
        [Test]
        public async Task FetchLatestAsyncShouldMapAndSortBitstampResponse()
        {
            const string json = """
        {
          "timestamp": "1700000000",
          "bids": [
            ["100.00", "2.5"],
            ["101.00", "1.0"]
          ],
          "asks": [
            ["103.00", "3.0"],
            ["102.00", "1.5"]
          ]
        }
        """;

            using var httpClient = CreateHttpClient(json);
            var source = new BitstampOrderBookSource(httpClient);

            var snapshot = await source.FetchLatestAsync();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Symbol, Is.EqualTo("BTC/EUR"));
                Assert.That(snapshot.Sequence, Is.EqualTo(0));

                Assert.That(snapshot.Bids, Has.Count.EqualTo(2));
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Bids[0].Price, Is.EqualTo(101m));
                Assert.That(snapshot.Bids[1].Price, Is.EqualTo(100m));

                Assert.That(snapshot.Asks, Has.Count.EqualTo(2));
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Asks[0].Price, Is.EqualTo(102m));
                Assert.That(snapshot.Asks[1].Price, Is.EqualTo(103m));

                Assert.That(snapshot.Bids[0].Quantity, Is.EqualTo(1m));
                Assert.That(snapshot.Asks[0].Quantity, Is.EqualTo(1.5m));
            }
        }

        [Test]
        public void FetchLatestAsyncShouldThrowWhenResponseIsEmpty()
        {
            using var httpClient = CreateHttpClient("null");
            var source = new BitstampOrderBookSource(httpClient);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => source.FetchLatestAsync());
        }

        [Test]
        public void FetchLatestAsyncShouldThrowWhenPriceLevelIsInvalid()
        {
            const string json = """
        {
          "timestamp": "1700000000",
          "bids": [
            ["invalid-price", "2.5"]
          ],
          "asks": [
            ["102.00", "1.5"]
          ]
        }
        """;

            using var httpClient = CreateHttpClient(json);
            var source = new BitstampOrderBookSource(httpClient);

            Assert.ThrowsAsync<JsonException>(
                () => source.FetchLatestAsync());
        }

        [Test]
        public void FetchLatestAsyncShouldThrowWhenBitstampReturnsFailureStatus()
        {
            using var httpClient = CreateHttpClient(
                "{}",
                HttpStatusCode.ServiceUnavailable);

            var source = new BitstampOrderBookSource(httpClient);

            Assert.ThrowsAsync<HttpRequestException>(
                () => source.FetchLatestAsync());
        }

        private static HttpClient CreateHttpClient(
            string responseContent,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new StubHttpMessageHandler(
                responseContent,
                statusCode);

            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://www.bitstamp.net"),
            };
        }

        private sealed class StubHttpMessageHandler(
            string responseContent,
            HttpStatusCode statusCode)
            : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
                    Assert.That(
                        request.RequestUri?.PathAndQuery,
                        Is.EqualTo("/api/v2/order_book/btceur/"));
                }

                var response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(
                        responseContent,
                        Encoding.UTF8,
                        "application/json"),
                };

                return Task.FromResult(response);
            }
        }
    }
}
