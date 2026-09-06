using BtcEurMarketDepth.Api.Endpoints;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.MarketData;

var builder = WebApplication.CreateBuilder(args);
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
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<IOrderBookProvider, BitstampOrderBookProvider>(client =>
{
    client.BaseAddress = new Uri("https://www.bitstamp.net");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IBuyQuoteCalculator, BuyQuoteCalculator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapMarketEndpoints();

app.UseCors("Frontend");

app.Run();
