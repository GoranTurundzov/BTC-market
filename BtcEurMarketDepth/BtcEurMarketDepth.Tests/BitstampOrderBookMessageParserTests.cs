using System.Text.Json;

using BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class BitstampOrderBookMessageParserTests
    {
        [Test]
        public void CreateSnapshotShouldMapAndSortBitstampResponse()
        {
            var response = new BitstampOrderBookResponse
            {
                Timestamp = "1700000000",
                Bids =
                [
                    ["100.00", "2.5"],
                    ["101.00", "1.0"],
                ],
                Asks =
                [
                    ["103.00", "3.0"],
                    ["102.00", "1.5"],
                ],
            };

            var snapshot = BitstampOrderBookMessageParser.CreateSnapshot(response, "BTC/EUR");

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
        public void CreateSnapshotShouldThrowWhenPriceLevelIsInvalid()
        {
            var response = new BitstampOrderBookResponse
            {
                Timestamp = "1700000000",
                Bids =
                [
                    ["invalid-price", "2.5"],
                ],
                Asks =
                [
                    ["102.00", "1.5"],
                ],
            };

            Assert.Throws<JsonException>(
                () => BitstampOrderBookMessageParser.CreateSnapshot(response, "BTC/EUR"));
        }

        [Test]
        public void CreateSnapshotShouldThrowWhenResponseIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => BitstampOrderBookMessageParser.CreateSnapshot(null!, "BTC/EUR"));
        }
    }
}
