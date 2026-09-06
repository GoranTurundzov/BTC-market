using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Domain.Model;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    /// <summary>
    /// Stores the most recently acquired order-book snapshot in memory.
    /// </summary>
    public class InMemoryOrderBookStore : IOrderBookStore
    {
        private OrderBookSnapshot? latestSnapshot;

        /// <summary>
        /// Returns the latest snapshot currently available.
        /// </summary>
        /// <returns>
        /// The latest snapshot, or null when no snapshot has been acquired yet.
        /// </returns>
        public OrderBookSnapshot? GetLatest()
        {
            return Volatile.Read(ref latestSnapshot);
        }

        /// <summary>
        /// Replaces the current snapshot with a newly acquired snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot to store.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the snapshot is null.
        /// </exception>
        public void SetLatest(OrderBookSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);

            Interlocked.Exchange(ref latestSnapshot, snapshot);
        }
    }
}
