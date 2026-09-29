using BtcEurMarketDepth.Api.MarketData;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.Configuration;
using BtcEurMarketDepth.Infrastructure.MarketData.Exchanges.Bitstamp;
using BtcEurMarketDepth.Infrastructure.MarketData.Processing;
using BtcEurMarketDepth.Infrastructure.MarketData.Stores;
using BtcEurMarketDepth.Infrastructure.Persistence;
using BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddSignalR();

            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("Cors:AllowedOrigins is not configured.");


            if (allowedOrigins.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one CORS origin must be configured.");
            }

            services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
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
        /// Registers database access, the live WebSocket market-data source,
        /// snapshot storage, and the background streaming service.
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

            services
                .AddOptions<BitstampWebSocketOptions>()
                .Bind(configuration.GetSection(BitstampWebSocketOptions.SectionName))
                .Validate(settings => Uri.TryCreate(
                    settings.Url,
                    UriKind.Absolute,
                    out var uri) && uri.Scheme == Uri.UriSchemeWss,
                    "BitstampWebSocket:Url must be an absolute wss:// URL.")
                .Validate(settings => Uri.TryCreate(
                    settings.RestApiUrl,
                    UriKind.Absolute,
                    out var restUri) && restUri.Scheme == Uri.UriSchemeHttps,
                    "BitstampWebSocket:RestApiUrl must be an absolute https:// URL.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.MarketSymbol),
                    "BitstampWebSocket:MarketSymbol is required.")
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.DisplaySymbol),
                    "BitstampWebSocket:DisplaySymbol is required.")
                .Validate(settings => settings.ReconnectDelaySeconds > 0,
                    "BitstampWebSocket:ReconnectDelaySeconds must be greater than zero.")
                .Validate(settings => settings.PublishIntervalMilliseconds > 0,
                    "BitstampWebSocket:PublishIntervalMilliseconds must be greater than zero.")
                .ValidateOnStart();

            services
                .AddHttpClient<BitstampOrderBookSource>((serviceProvider, client) =>
                {
                    var settings = serviceProvider
                        .GetRequiredService<IOptions<BitstampWebSocketOptions>>()
                        .Value;

                    client.BaseAddress = new Uri(
                        settings.RestApiUrl.TrimEnd('/') + "/");
                })
                .AddStandardResilienceHandler();

            services.AddSingleton<IOrderBookSnapshotAuditRepository, OrderBookSnapshotAuditRepository>();

            services.AddSingleton<IOrderBookSource>(serviceProvider =>
                serviceProvider.GetRequiredService<BitstampOrderBookSource>());

            services.AddSingleton<IClientWebSocketFactory, ClientWebSocketFactory>();

            services.AddSingleton<IOrderBookStore, OrderBookStore>();

            services.AddHostedService<OrderBookStreamingService>();

            return services;
        }
    }
}
