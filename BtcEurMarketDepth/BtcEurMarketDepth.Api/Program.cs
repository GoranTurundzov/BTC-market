using BtcEurMarketDepth.Api.Configuration;
using BtcEurMarketDepth.Api.Endpoints;
using BtcEurMarketDepth.Api.Hubs;

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
