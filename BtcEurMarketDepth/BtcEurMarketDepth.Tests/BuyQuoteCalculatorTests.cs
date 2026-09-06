using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class BuyQuoteCalculatorTests
    {
        private readonly BuyQuoteCalculator calculator = new();

        [Test]
        public void CalculateShouldFullyFillOrderUsingMultipleAskLevels()
        {
            var snapshot = CreateSnapshot(
                new PriceLevel(100m, 2m),
                new PriceLevel(110m, 3m));

            var result = calculator.Calculate(snapshot, 4m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RequestedQuantity, Is.EqualTo(4m));
                Assert.That(result.FilledQuantity, Is.EqualTo(4m));
                Assert.That(result.RemainingQuantity, Is.EqualTo(0m));
                Assert.That(result.TotalCost, Is.EqualTo(420m));
                Assert.That(result.AveragePrice, Is.EqualTo(105m));
                Assert.That(result.BestAsk, Is.EqualTo(100m));
                Assert.That(result.IsFullyFillable, Is.True);
                Assert.That(result.SnapshotSequence, Is.EqualTo(1));
            }
        }

        [Test]
        public void CalculateShouldReturnPartialFillWhenLiquidityIsInsufficient()
        {
            var snapshot = CreateSnapshot(
                new PriceLevel(100m, 2m),
                new PriceLevel(110m, 3m));

            var result = calculator.Calculate(snapshot, 10m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RequestedQuantity, Is.EqualTo(10m));
                Assert.That(result.FilledQuantity, Is.EqualTo(5m));
                Assert.That(result.RemainingQuantity, Is.EqualTo(5m));
                Assert.That(result.TotalCost, Is.EqualTo(530m));
                Assert.That(result.AveragePrice, Is.EqualTo(106m));
                Assert.That(result.IsFullyFillable, Is.False);
            }
        }

        [Test]
        public void CalculateShouldUseBestAskWhenOrderFitsWithinFirstLevel()
        {
            var snapshot = CreateSnapshot(
                new PriceLevel(100m, 5m),
                new PriceLevel(110m, 5m));

            var result = calculator.Calculate(snapshot, 2m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.FilledQuantity, Is.EqualTo(2m));
                Assert.That(result.RemainingQuantity, Is.EqualTo(0m));
                Assert.That(result.TotalCost, Is.EqualTo(200m));
                Assert.That(result.AveragePrice, Is.EqualTo(100m));
                Assert.That(result.IsFullyFillable, Is.True);
            }
        }

        [Test]
        public void CalculateShouldRejectZeroQuantity()
        {
            var snapshot = CreateSnapshot(
                new PriceLevel(100m, 5m));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => calculator.Calculate(snapshot, 0m));
        }

        [Test]
        public void CalculateShouldReturnUnfilledQuoteWhenThereAreNoAsks()
        {
            var snapshot = CreateSnapshot();

            var result = calculator.Calculate(snapshot, 2m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.RequestedQuantity, Is.EqualTo(2m));
                Assert.That(result.FilledQuantity, Is.EqualTo(0m));
                Assert.That(result.RemainingQuantity, Is.EqualTo(2m));
                Assert.That(result.TotalCost, Is.EqualTo(0m));
                Assert.That(result.AveragePrice, Is.Null);
                Assert.That(result.BestAsk, Is.Null);
                Assert.That(result.IsFullyFillable, Is.False);
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CalculateShouldRejectNonPositiveQuantity(decimal quantity)
        {
            var snapshot = CreateSnapshot(new PriceLevel(100m, 5m));

            Assert.Throws<ArgumentOutOfRangeException>(() => calculator.Calculate(snapshot, quantity));
        }


        private static OrderBookSnapshot CreateSnapshot(
            params PriceLevel[] asks)
        {
            return new OrderBookSnapshot(
                symbol: "BTC/EUR",
                acquiredAt: DateTimeOffset.UtcNow,
                bids:
                [
                    new PriceLevel(90m, 5m),
                ],
                asks: asks,
                sequence: 1);
        }
    }
}
