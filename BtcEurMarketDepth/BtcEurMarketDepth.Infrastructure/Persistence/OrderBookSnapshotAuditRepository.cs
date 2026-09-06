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
    }
}
