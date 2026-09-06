using BtcEurMarketDepth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Infrastructure.Persistence
{
    public class MarketDepthDbContext(DbContextOptions<MarketDepthDbContext> options)
     : DbContext(options)
    {
        public DbSet<OrderBookSnapshotAudit> OrderBookSnapshots => Set<OrderBookSnapshotAudit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<OrderBookSnapshotAudit>();

            entity.ToTable("order_book_snapshots");

            entity.HasKey(snapshot => snapshot.Id);

            entity.Property(snapshot => snapshot.Id)
                .ValueGeneratedOnAdd();

            entity.Property(snapshot => snapshot.Symbol)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(snapshot => snapshot.AcquiredAt)
                .IsRequired();

            entity.Property(snapshot => snapshot.RecordedAt)
                .IsRequired();

            entity.Property(snapshot => snapshot.BestBid)
                .HasPrecision(20, 8);

            entity.Property(snapshot => snapshot.BestAsk)
                .HasPrecision(20, 8);

            entity.Property(snapshot => snapshot.BidsJson)
                .IsRequired();

            entity.Property(snapshot => snapshot.AsksJson)
                .IsRequired();

            entity.HasIndex(snapshot => new
            {
                snapshot.Symbol,
                snapshot.AcquiredAt
            });
        }
    }
}
