# BTC/EUR Market Depth

A real-time BTC/EUR order-book application using Bitstamp market data. The backend reconstructs the order book from Bitstamp's REST bootstrap and WebSocket diff feed, persists audit snapshots to SQL Server, and broadcasts live updates to the Vue frontend through SignalR.

## Features

- Live BTC/EUR order-book updates
- Bid/ask market-depth line chart
- Current, 1H, 1D, 1W, 1M, 1Y, YTD, and ALL chart ranges
- Custom historical From/To date-time search
- Current chart depth controls in 500-level increments
- Buy-quote calculation from ask-side liquidity
- Partial-fill detection and remaining requested quantity
- SQL Server audit history for acquired snapshots
- SignalR live notifications
- HTTP resilience for Bitstamp REST requests
- Backend and frontend tests
- Docker Compose support

## Technology

### Backend

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core with SQL Server
- SignalR
- Bitstamp REST and WebSocket feeds
- `Microsoft.Extensions.Http.Resilience`
- NUnit

### Frontend

- Vue 3
- TypeScript
- Vite
- Pinia
- ECharts and `vue-echarts`
- Tailwind CSS
- Vitest and Vue Test Utils

## Project structure

```text
BtcEurMarketDepth/
├── BtcEurMarketDepth.Api/
├── BtcEurMarketDepth.Application/
├── BtcEurMarketDepth.Domain/
├── BtcEurMarketDepth.Infrastructure/
├── BtcEurMarketDepth.Tests/
├── frontend/
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
└── BtcEurMarketDepth.slnx
```

## Run with Docker Compose

Requirements:

- Docker Desktop
- .NET 10 SDK if migrations will be run from the host
- Node.js 22 or later for frontend development outside Docker

From the repository root:

```powershell
docker compose up -d --build
```

Services:

| Service | Address |
|---|---|
| Frontend | http://localhost:5173 |
| API | http://localhost:5272 |
| SQL Server | localhost,1433 |

The Docker SQL Server credentials are intended for local development only:

```text
User: sa
Password: ASDqwe123!@#
Database: BtcEurMarketDepth
```

### Apply migrations

Run from the repository root. The CLI form works in ordinary PowerShell and does not require the Visual Studio Package Manager Console:

```powershell
dotnet ef database update `
  --project .\BtcEurMarketDepth.Infrastructure `
  --startup-project .\BtcEurMarketDepth.Api
```

To create a new migration after changing the EF model:

```powershell
dotnet ef migrations add AddAcquiredAtIndex `
  --project .\BtcEurMarketDepth.Infrastructure `
  --startup-project .\BtcEurMarketDepth.Api
```

Keep the existing `InitialCreate` migration. New migrations are applied after it.

If `dotnet ef` is not installed:

```powershell
dotnet tool install --global dotnet-ef
```

### Docker commands

```powershell
docker compose ps
docker compose logs -f api
docker compose logs -f frontend
docker compose logs -f sqlserver
docker compose down
```

The Compose file does not configure a persistent SQL Server volume. Removing the SQL Server container can remove its database data. Add a named volume before relying on Docker for persistent development data.

## Run the API from Visual Studio

The development configuration expects the Docker SQL Server container. Start only the database:

```powershell
docker compose up -d sqlserver
```

Then run `BtcEurMarketDepth.Api` from Visual Studio. The connection is configured in:

```text
BtcEurMarketDepth.Api/appsettings.Development.json
```

It uses `localhost,1433` with SQL Server authentication. Do not select `(localdb)\\MSSQLLocalDB` unless you intentionally change the development connection string back to LocalDB.

## Run the frontend separately

From the `frontend` directory:

```powershell
npm ci
npm run dev
```

The development API base URL is configured in `frontend/.env.development`.

## API endpoints

Latest in-memory order book:

```text
GET /api/market/order-book
```

Historical snapshots:

```text
GET /api/market/order-book/history?from={from}&to={to}
```

Buy quote using the live order book:

```text
GET /api/market/quote?quantity={btcAmount}
```

Buy quote using the closest stored snapshot at a timestamp:

```text
GET /api/market/quote?quantity={btcAmount}&at={timestamp}
```

SignalR hub:

```text
/hubs/market
```

## Data flow

1. Bitstamp REST provides the initial order-book snapshot.
2. Bitstamp WebSocket diff messages update the reconstructed order book.
3. The streaming service publishes at the configured cadence.
4. The published snapshot is saved to SQL Server.
5. The latest snapshot is stored in memory.
6. SignalR broadcasts the snapshot to connected frontend clients.

The configured publication interval is controlled by:

```json
"PublishIntervalMilliseconds": 5000
```

This controls audit and client publication frequency; it is not a polling interval for the Bitstamp WebSocket.

## Database history and retention

Full order-book snapshots are large. Persisting one every second can produce 86,400 rows per day, so the publication interval and retention policy should be chosen deliberately for production.

The audit table is:

```text
dbo.order_book_snapshots
```

History queries use timestamp indexes, but long ranges still require loading and deserializing bid/ask JSON. A production-scale implementation should add retention and downsampled history for long chart ranges.

## Tests and builds

Backend tests:

```powershell
dotnet test BtcEurMarketDepth.slnx
```

Frontend tests:

```powershell
cd frontend
npm test
```

Frontend type-check:

```powershell
npm run type-check
```

Frontend production build:

```powershell
npm run build
```

Backend release build:

```powershell
dotnet build BtcEurMarketDepth.slnx --configuration Release
```
