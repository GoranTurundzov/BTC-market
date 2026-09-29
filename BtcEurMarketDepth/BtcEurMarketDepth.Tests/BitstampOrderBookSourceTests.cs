using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using BtcEurMarketDepth.Infrastructure.Configuration;
using BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp;
using BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class BitstampOrderBookSourceTests
    {
        [Test]
        public async Task ReadSnapshotsAsyncShouldSendSubscriptionAndYieldSnapshot()
        {
            var socket = new FakeWebSocketClient();

            socket.EnqueueTextMessage(
                """
            {
              "event": "data",
              "channel": "diff_order_book_btceur",
              "event_id": "2",
              "pre_event_id": "1",
              "data": {
                "timestamp": "1700000000",
                "bids": [
                  ["100.00", "2.5"]
                ],
                "asks": [
                  ["101.00", "1.0"]
                ]
              }
            }
            """);

            var source = CreateSource(socket);

            using var cancellationSource = new CancellationTokenSource();

            await using var enumerator = source
                .ReadSnapshotsAsync(cancellationSource.Token)
                .GetAsyncEnumerator(cancellationSource.Token);

            var hasSnapshot = await enumerator.MoveNextAsync();

            Assert.That(hasSnapshot, Is.True);

            var snapshot = enumerator.Current;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Symbol, Is.EqualTo("BTC/EUR"));
                Assert.That(snapshot.Bids, Has.Count.EqualTo(1));
                Assert.That(snapshot.Asks, Has.Count.EqualTo(1));
                Assert.That(snapshot.Bids[0].Price, Is.EqualTo(99m));
                Assert.That(snapshot.Bids[0].Quantity, Is.EqualTo(2.5m));
                Assert.That(snapshot.Asks[0].Price, Is.EqualTo(102m));
                Assert.That(snapshot.Asks[0].Quantity, Is.EqualTo(1m));
            }

            Assert.That(await enumerator.MoveNextAsync(), Is.True);
            Assert.That(enumerator.Current.Bids[0].Price, Is.EqualTo(100m));
            Assert.That(enumerator.Current.Asks[0].Price, Is.EqualTo(101m));

            Assert.That(socket.SentMessages, Has.Count.EqualTo(1));

            using var subscriptionDocument =
                JsonDocument.Parse(socket.SentMessages[0]);

            var root = subscriptionDocument.RootElement;

            Assert.That(
                root.GetProperty("event").GetString(),
                Is.EqualTo("bts:subscribe"));

            Assert.That(
                root.GetProperty("data")
                    .GetProperty("channel")
                    .GetString(),
                Is.EqualTo("diff_order_book_btceur"));

            cancellationSource.Cancel();
        }

        [Test]
        public async Task ReadSnapshotsAsyncShouldIgnoreSubscriptionAcknowledgement()
        {
            var socket = new FakeWebSocketClient();

            socket.EnqueueTextMessage(
                """
            {
              "event": "bts:subscription_succeeded",
              "channel": "diff_order_book_btceur",
              "data": {}
            }
            """);

            socket.EnqueueTextMessage(
                """
            {
              "event": "data",
              "channel": "diff_order_book_btceur",
              "event_id": "2",
              "pre_event_id": "1",
              "data": {
                "timestamp": "1700000000",
                "bids": [
                  ["100.00", "2.5"]
                ],
                "asks": [
                  ["101.00", "1.0"]
                ]
              }
            }
            """);

            var source = CreateSource(socket);

            using var cancellationSource = new CancellationTokenSource();

            await using var enumerator = source
                .ReadSnapshotsAsync(cancellationSource.Token)
                .GetAsyncEnumerator(cancellationSource.Token);

            var hasSnapshot = await enumerator.MoveNextAsync();

            Assert.That(hasSnapshot, Is.True);
            Assert.That(enumerator.Current.Bids, Has.Count.EqualTo(1));
            Assert.That(enumerator.Current.Asks, Has.Count.EqualTo(1));

            cancellationSource.Cancel();
        }

        [Test]
        public async Task ReadSnapshotsAsyncShouldIgnoreDifferentChannel()
        {
            var socket = new FakeWebSocketClient();

            socket.EnqueueTextMessage(
                """
        {
          "event": "data",
          "channel": "diff_order_book_ethusd",
          "data": {
            "timestamp": "1700000000",
            "bids": [
              ["100.00", "2.5"]
            ],
            "asks": [
              ["101.00", "1.0"]
            ]
          }
        }
        """);

            socket.EnqueueTextMessage(
                """
        {
          "event": "data",
          "channel": "diff_order_book_btceur",
          "event_id": "2",
          "pre_event_id": "1",
          "data": {
            "timestamp": "1700000000",
            "bids": [
              ["100.00", "2.5"]
            ],
            "asks": [
              ["101.00", "1.0"]
            ]
          }
        }
        """);

            var source = CreateSource(socket);

            using var cancellationSource = new CancellationTokenSource();

            await using var enumerator = source
                .ReadSnapshotsAsync(cancellationSource.Token)
                .GetAsyncEnumerator(cancellationSource.Token);

            var hasSnapshot = await enumerator.MoveNextAsync();

            Assert.That(hasSnapshot, Is.True);
            Assert.That(enumerator.Current.Symbol, Is.EqualTo("BTC/EUR"));
            Assert.That(enumerator.Current.Bids, Has.Count.EqualTo(1));
            Assert.That(enumerator.Current.Asks, Has.Count.EqualTo(1));

            cancellationSource.Cancel();
        }

        private static BitstampOrderBookSource CreateSource(
            IWebSocketClient socket)
        {
            var options = Options.Create(
                new BitstampWebSocketOptions
                {
                    Url = "wss://ws.bitstamp.net",
                    RestApiUrl = "https://www.bitstamp.net/api/v2/",
                    MarketSymbol = "btceur",
                    DisplaySymbol = "BTC/EUR",
                    ReconnectDelaySeconds = 1
                });

            var factory = new FakeClientWebSocketFactory(socket);

            var httpClient = new HttpClient(
                new FakeHttpMessageHandler())
            {
                BaseAddress = new Uri("https://www.bitstamp.net/api/v2/")
            };

            return new BitstampOrderBookSource(
                NullLogger<BitstampOrderBookSource>.Instance,
                options,
                factory,
                httpClient);

        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "timestamp": "1700000000",
                          "microtimestamp": "1700000000000000",
                          "bids": [["99.00", "2.5"]],
                          "asks": [["102.00", "1.0"]]
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };

                return Task.FromResult(response);
            }
        }
    }
}
