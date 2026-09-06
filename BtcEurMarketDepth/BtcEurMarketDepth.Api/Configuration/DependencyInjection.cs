using BtcEurMarketDepth.Api.MarketData;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.MarketData;
using BtcEurMarketDepth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BtcEurMarketDepth.Api.Configuration
{
    public static class DependencyInjection
    {
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

        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddSingleton<
                IBuyQuoteCalculator,
                BuyQuoteCalculator>();

            return services;
        }

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
