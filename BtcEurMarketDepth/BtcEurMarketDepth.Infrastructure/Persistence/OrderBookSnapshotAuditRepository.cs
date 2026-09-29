using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;
using BtcEurMarketDepth.Infrastructure.Persistence.Entities;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;

namespace BtcEurMarketDepth.Infrastructure.Persistence
{
    public class OrderBookSnapshotAuditRepository(
    IDbContextFactory<MarketDepthDbContext> dbContextFactory)
    : IOrderBookSnapshotAuditRepository
    {
        public async Task SaveAsync(
            OrderBookSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            await using var dbContext =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var auditSnapshot = new OrderBookSnapshotAudit
            {
                Symbol = snapshot.Symbol,
                Sequence = snapshot.Sequence,
                AcquiredAt = snapshot.AcquiredAt,
                RecordedAt = DateTimeOffset.UtcNow,
                BestBid = snapshot.Bids.Count > 0 ? snapshot.Bids[0].Price : null,
                BestAsk = snapshot.Asks.Count > 0 ? snapshot.Asks[0].Price : null,
                BidsJson = JsonSerializer.Serialize(snapshot.Bids),
                AsksJson = JsonSerializer.Serialize(snapshot.Asks)
            };

            dbContext.OrderBookSnapshots.Add(auditSnapshot);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OrderBookSnapshot>> GetHistoryAsync(
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
        {
            var snapshots = await dbContextFactory
                .CreateDbContextAsync(cancellationToken);

            await using (snapshots)
            {
                var auditSnapshots = await snapshots.OrderBookSnapshots
                    .AsNoTracking()
                    .Where(snapshot =>
                        snapshot.AcquiredAt >= from &&
                        snapshot.AcquiredAt <= to)
                    .OrderBy(snapshot => snapshot.AcquiredAt)
                    .ToListAsync(cancellationToken);

                return auditSnapshots
                    .Select(MapSnapshot)
                    .ToArray();
            }
        }

        public async Task<OrderBookSnapshot?> GetClosestAsync(
            DateTimeOffset timestamp,
            CancellationToken cancellationToken = default)
        {
            await using var dbContext =
                await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var auditSnapshot = await dbContext.OrderBookSnapshots
                .AsNoTracking()
                .Where(snapshot => snapshot.AcquiredAt <= timestamp)
                .OrderByDescending(snapshot => snapshot.AcquiredAt)
                .FirstOrDefaultAsync(cancellationToken);

            return auditSnapshot is null
                ? null
                : MapSnapshot(auditSnapshot);
        }

        private static OrderBookSnapshot MapSnapshot(
            OrderBookSnapshotAudit auditSnapshot)
        {
            var bids = JsonSerializer.Deserialize<List<PriceLevel>>(
                auditSnapshot.BidsJson) ?? [];

            var asks = JsonSerializer.Deserialize<List<PriceLevel>>(
                auditSnapshot.AsksJson) ?? [];

            return new OrderBookSnapshot(
                auditSnapshot.Symbol,
                auditSnapshot.AcquiredAt,
                bids,
                asks,
                auditSnapshot.Sequence);
        }
    }
}
