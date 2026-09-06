using BtcEurMarketDepth.Application.MarketData;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BtcEurMarketDepth.Infrastructure.MarketData
{
    public sealed class OrderBookPollingService(
    IOrderBookSource orderBookSource,
    IOrderBookStore orderBookStore,
    IOrderBookUpdateNotifier orderBookUpdateNotifier,
    IOrderBookSnapshotAuditRepository auditRepository,
    ILogger<OrderBookPollingService> logger) : BackgroundService
    {
        private readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(PollingInterval);

            await FetchAndStoreAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await FetchAndStoreAsync(stoppingToken);
            }
        }

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
