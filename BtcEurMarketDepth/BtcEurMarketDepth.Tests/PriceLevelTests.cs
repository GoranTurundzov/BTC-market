using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class PriceLevelTests
    {
        [Test]
        public void ConstructorShouldCreatePriceLevelForValidValues()
        {
            var priceLevel = new PriceLevel(68_500.25m, 0.125m);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(priceLevel.Price, Is.EqualTo(68_500.25m));
                Assert.That(priceLevel.Quantity, Is.EqualTo(0.125m));
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ConstructorShouldRejectNonPositivePrice(decimal price)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PriceLevel(price, 1m));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ConstructorShouldRejectNonPositiveQuantity(decimal quantity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PriceLevel(68_500m, quantity));
        }
    }
}
