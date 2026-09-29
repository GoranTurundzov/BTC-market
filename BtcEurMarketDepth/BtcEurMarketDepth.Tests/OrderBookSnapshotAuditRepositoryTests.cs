using BtcEurMarketDepth.Domain.Model;
using BtcEurMarketDepth.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BtcEurMarketDepth.Tests
{
    [TestFixture]
    public sealed class OrderBookSnapshotAuditRepositoryTests
    {
        [Test]
        public async Task GetHistoryAsyncShouldReturnSnapshotsWithinRequestedRange()
        {
            var options = CreateOptions();
            await using var context = new MarketDepthDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new OrderBookSnapshotAuditRepository(
                new TestDbContextFactory(options));

            var first = CreateSnapshot(DateTimeOffset.UtcNow.AddMinutes(-10), 1);
            var second = CreateSnapshot(DateTimeOffset.UtcNow.AddMinutes(-5), 2);
            var outsideRange = CreateSnapshot(DateTimeOffset.UtcNow, 3);

            await repository.SaveAsync(first);
            await repository.SaveAsync(second);
            await repository.SaveAsync(outsideRange);

            var result = await repository.GetHistoryAsync(
                first.AcquiredAt.AddSeconds(-1),
                second.AcquiredAt.AddSeconds(1));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result[0].Sequence, Is.EqualTo(1));
                Assert.That(result[1].Sequence, Is.EqualTo(2));
                Assert.That(result[0].Bids[0].Price, Is.EqualTo(100m));
            }
        }

        [Test]
        public async Task GetClosestAsyncShouldReturnLatestSnapshotBeforeTimestamp()
        {
            var options = CreateOptions();
            await using var context = new MarketDepthDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var repository = new OrderBookSnapshotAuditRepository(
                new TestDbContextFactory(options));

            var first = CreateSnapshot(DateTimeOffset.UtcNow.AddMinutes(-10), 1);
            var second = CreateSnapshot(DateTimeOffset.UtcNow.AddMinutes(-5), 2);

            await repository.SaveAsync(first);
            await repository.SaveAsync(second);

            var result = await repository.GetClosestAsync(
                second.AcquiredAt.AddSeconds(1));

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Sequence, Is.EqualTo(2));
        }

        private static DbContextOptions<MarketDepthDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<MarketDepthDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static OrderBookSnapshot CreateSnapshot(
            DateTimeOffset acquiredAt,
            long sequence)
        {
            return new OrderBookSnapshot(
                "BTC/EUR",
                acquiredAt,
                [new PriceLevel(100m, 1m)],
                [new PriceLevel(101m, 2m)],
                sequence);
        }

        private sealed class TestDbContextFactory(
            DbContextOptions<MarketDepthDbContext> options)
            : IDbContextFactory<MarketDepthDbContext>
        {
            public MarketDepthDbContext CreateDbContext()
            {
                return new MarketDepthDbContext(options);
            }

            public Task<MarketDepthDbContext> CreateDbContextAsync(
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new MarketDepthDbContext(options));
            }
        }
    }
}
