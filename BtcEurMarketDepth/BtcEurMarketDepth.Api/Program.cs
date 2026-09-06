using BtcEurMarketDepth.Api.Endpoints;
using BtcEurMarketDepth.Api.Hubs;
using BtcEurMarketDepth.Api.MarketData;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.MarketData;
using BtcEurMarketDepth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("MarketDepth") ?? throw new InvalidOperationException("The MarketDepth database connection string is not configured.");
// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
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

builder.Services.AddDbContextFactory<MarketDepthDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IOrderBookSnapshotAuditRepository, OrderBookSnapshotAuditRepository>();

builder.Services.AddHttpClient<IOrderBookSource, BitstampOrderBookSource>(client =>
{
    client.BaseAddress = new Uri("https://www.bitstamp.net");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSignalR();

builder.Services.AddSingleton<IOrderBookUpdateNotifier, SignalROrderBookUpdateNotifier>();
builder.Services.AddSingleton<IOrderBookStore, InMemoryOrderBookStore>();
builder.Services.AddSingleton<IBuyQuoteCalculator, BuyQuoteCalculator>();
builder.Services.AddHostedService<OrderBookPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapMarketEndpoints();

app.UseCors("Frontend");
app.MapHub<MarketHub>("/hubs/market");

app.Run();
