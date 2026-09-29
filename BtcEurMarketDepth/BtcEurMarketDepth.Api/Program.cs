using BtcEurMarketDepth.Api.Configuration;
using BtcEurMarketDepth.Api.Endpoints;
using BtcEurMarketDepth.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.MapMarketEndpoints();
app.MapHub<MarketHub>("/hubs/market");

app.Run();
