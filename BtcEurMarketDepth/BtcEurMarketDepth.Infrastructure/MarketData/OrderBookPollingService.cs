using BtcEurMarketDepth.Application.MarketData;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    /// <summary>
    /// Periodically acquires order-book snapshots, persists them for auditing,
    /// stores the latest snapshot, and notifies connected clients.
    /// </summary>
    public sealed class OrderBookPollingService(
    IOrderBookSource orderBookSource,
    IOrderBookStore orderBookStore,
    IOrderBookUpdateNotifier orderBookUpdateNotifier,
    IOrderBookSnapshotAuditRepository auditRepository,
    ILogger<OrderBookPollingService> logger) : BackgroundService
    {
        private readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Starts the polling loop and performs the first acquisition immediately.
        /// Subsequent acquisitions occur at the configured polling interval.
        /// </summary>
        /// <param name="stoppingToken">
        /// Token used to stop the background service.
        /// </param>
        /// <returns>
        /// A task representing the lifetime of the polling service.
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollingInterval);

            await FetchAndStoreAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FetchAndStoreAsync(stoppingToken);
            }
        }

        /// <summary>
        /// Acquires one snapshot and sends it through the application pipeline.
        /// A failure is logged so that the polling loop can continue with the
        /// next scheduled acquisition.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to cancel the acquisition and processing operation.
        /// </param>
        private async Task FetchAndStoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                var snapshot = await orderBookSource.FetchLatestAsync(cancellationToken);

                await auditRepository.SaveAsync(snapshot, cancellationToken);

                orderBookStore.SetLatest(snapshot);

                await orderBookUpdateNotifier.NotifyAsync(snapshot, cancellationToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                    "Updated {Symbol} order book with {BidCount} bids and {AskCount} asks at {AcquiredAt}.",
                    snapshot.Symbol,
                    snapshot.Bids.Count,
                    snapshot.Asks.Count,
                    snapshot.AcquiredAt);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to acquire and process the latest order book snapshot.");
            }
        }
    }
}
