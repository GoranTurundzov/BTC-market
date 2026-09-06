using BtcEurMarketDepth.Api.Configuration;
using BtcEurMarketDepth.Api.Endpoints;
using BtcEurMarketDepth.Api.Hubs;
using BtcEurMarketDepth.Api.MarketData;
using BtcEurMarketDepth.Application.MarketData;
using BtcEurMarketDepth.Application.Quotes;
using BtcEurMarketDepth.Infrastructure.MarketData;
using BtcEurMarketDepth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApiServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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
