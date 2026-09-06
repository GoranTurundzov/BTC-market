using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class OrderBookSnapshotTests
    {
        [Test]
        public void ConstructorShouldCreateSnapshotForValidData()
        {
            var snapshot = CreateSnapshot();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Symbol, Is.EqualTo("BTC/EUR"));
                Assert.That(snapshot.Bids, Has.Count.EqualTo(2));
                Assert.That(snapshot.Asks, Has.Count.EqualTo(2));
                Assert.That(snapshot.Sequence, Is.EqualTo(1));
            }
        }

        [Test]
        public void ConstructorShouldRejectEmptySymbol()
        {
            Assert.Throws<ArgumentException>(
                () => CreateSnapshot(symbol: string.Empty));
        }

        [Test]
        public void ConstructorShouldRejectBidsThatAreNotOrderedDescending()
        {
            var bids = new[]
            {
            new PriceLevel(90m, 1m),
            new PriceLevel(100m, 1m),
        };

            Assert.Throws<ArgumentException>(
                () => CreateSnapshot(bids: bids));
        }

        [Test]
        public void ConstructorShouldRejectAsksThatAreNotOrderedAscending()
        {
            var asks = new[]
            {
            new PriceLevel(110m, 1m),
            new PriceLevel(100m, 1m),
        };

            Assert.Throws<ArgumentException>(
                () => CreateSnapshot(asks: asks));
        }

        [Test]
        public void ConstructorShouldRejectDuplicateBidPrices()
        {
            var bids = new[]
            {
            new PriceLevel(100m, 1m),
            new PriceLevel(100m, 2m),
        };

            Assert.Throws<ArgumentException>(
                () => CreateSnapshot(bids: bids));
        }

        [Test]
        public void ConstructorShouldRejectDuplicateAskPrices()
        {
            var asks = new[]
            {
            new PriceLevel(100m, 1m),
            new PriceLevel(100m, 2m),
        };

            Assert.Throws<ArgumentException>(
                () => CreateSnapshot(asks: asks));
        }

        [Test]
        public void ConstructorShouldCopyInputCollections()
        {
            var bids = new[]
            {
            new PriceLevel(100m, 1m),
        };

            var snapshot = CreateSnapshot(bids: bids);

            bids[0] = new PriceLevel(50m, 10m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(snapshot.Bids[0].Price, Is.EqualTo(100m));
                Assert.That(snapshot.Bids[0].Quantity, Is.EqualTo(1m));
            }
        }

        private static OrderBookSnapshot CreateSnapshot(
            string symbol = "BTC/EUR",
            PriceLevel[]? bids = null,
            PriceLevel[]? asks = null)
        {
            return new OrderBookSnapshot(
                symbol: symbol,
                acquiredAt: DateTimeOffset.UtcNow,
                bids: bids ??
                [
                    new PriceLevel(100m, 1m),
                new PriceLevel(90m, 2m),
                ],
                asks: asks ??
                [
                    new PriceLevel(110m, 1m),
                new PriceLevel(120m, 2m),
                ],
                sequence: 1);
        }
    }
}
