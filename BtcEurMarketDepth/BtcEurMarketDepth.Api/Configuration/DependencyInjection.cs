using BtcEurMarketDepth.Api.MarketData;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.MarketData;
using BtcEurMarketDepth.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace BtcEurMarketDepth.Api.Configuration
{
    /// <summary>
    /// Registers the application's services and infrastructure dependencies.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers services used directly by the API layer, including SignalR,
        /// CORS, OpenAPI, and the market-update notifier.
        /// </summary>
        /// <param name="services">
        /// The application's dependency-injection service collection.
        /// </param>
        /// <returns>
        /// The same service collection for chaining additional registrations.
        /// </returns>
        public static IServiceCollection AddApiServices(
            this IServiceCollection services)
        {
            services.AddOpenApi();
            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddSingleton<
                IOrderBookUpdateNotifier,
                SignalROrderBookUpdateNotifier>();

            return services;
        }

        /// <summary>
        /// Registers application-level services such as quote calculation.
        /// </summary>
        /// <param name="services">
        /// The application's dependency-injection service collection.
        /// </param>
        /// <returns>
        /// The same service collection for chaining additional registrations.
        /// </returns>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddSingleton<
                IBuyQuoteCalculator,
                BuyQuoteCalculator>();

            return services;
        }

        /// <summary>
        /// Registers database access, external market-data access,
        /// resilience policies, snapshot storage, and background polling.
        /// </summary>
        /// <param name="services">
        /// The application's dependency-injection service collection.
        /// </param>
        /// <param name="configuration">
        /// Application configuration containing the database connection string.
        /// </param>
        /// <returns>
        /// The same service collection for chaining additional registrations.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the MarketDepth database connection string is missing.
        /// </exception>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MarketDepth")
                ?? throw new InvalidOperationException(
                    "The MarketDepth database connection string is not configured.");

            services.AddDbContextFactory<MarketDepthDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddSingleton<
                IOrderBookSnapshotAuditRepository,
                OrderBookSnapshotAuditRepository>();

            services
                .AddHttpClient<IOrderBookSource, BitstampOrderBookSource>(client =>
                {
                    client.BaseAddress = new Uri("https://www.bitstamp.net");
                })
                .AddStandardResilienceHandler(options =>
                {
                    options.Retry.MaxRetryAttempts = 3;
                    options.Retry.Delay = TimeSpan.FromSeconds(1);
                    options.Retry.UseJitter = true;

                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                });

            services.AddSingleton<
                IOrderBookStore,
                InMemoryOrderBookStore>();

            services.AddHostedService<OrderBookPollingService>();

            return services;
        }
    }
}
