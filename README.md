# BTC/EUR Market Depth

A real-time BTC/EUR order-book visualization and buy-quote calculator.

The application continuously retrieves market data from Bitstamp, stores acquired order-book snapshots for auditing, and updates the frontend through SignalR.

## Features

- Real-time BTC/EUR order-book updates
- Bid and ask tables
- Interactive market-depth bar chart
- Chart zooming and loading deeper price levels
- BTC buy-quote calculation
- Partial-fill support when the requested quantity cannot be fully purchased
- SQL Server audit log for acquired order-book snapshots
- SignalR notifications for live frontend updates
- Automatic HTTP resilience and retry handling
- Backend and frontend tests
- Docker Compose support

## Technology Stack

### Backend

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQL Server
- SignalR
- `Microsoft.Extensions.Http.Resilience`
- NUnit

### Frontend

- Vue 3
- TypeScript
- Vite
- ECharts
- Tailwind CSS
- Vitest
- Vue Test Utils

## Project Structure

```text
BtcEurMarketDepth/
├── BtcEurMarketDepth.Api/
├── BtcEurMarketDepth.Application/
├── BtcEurMarketDepth.Domain/
├── BtcEurMarketDepth.Infrastructure/
├── BtcEurMarketDepth.Tests/
├── frontend/
├── .dockerignore
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
└── BtcEurMarketDepth.slnx
```

## Run with Docker Compose

### Requirements

- Docker Desktop
- Git
- Internet access for Bitstamp and Docker image downloads

### Start the application

From the `BtcEurMarketDepth` directory:

```powershell
docker compose up -d --build
```

This starts:

- Vue frontend
- .NET API
- SQL Server

Check the containers:

```powershell
docker compose ps
```

### Apply the database migration

The SQL Server container starts with an empty database. Apply the EF Core migration from the `BtcEurMarketDepth` directory:

```powershell
$env:ConnectionStrings__MarketDepth = "Server=127.0.0.1,1433;Database=BtcEurMarketDepth;User Id=sa;Password=ASDqwe123!@#;TrustServerCertificate=True"

dotnet ef database update `
  --project BtcEurMarketDepth.Infrastructure `
  --startup-project BtcEurMarketDepth.Api

Remove-Item Env:ConnectionStrings__MarketDepth
```

After the migration completes, the following database objects are created:

- `dbo.order_book_snapshots`
- `dbo.__EFMigrationsHistory`

### Open the application

```text
http://localhost:5173
```

### Service URLs

Frontend:

```text
http://localhost:5173
```

Backend API:

```text
http://localhost:5272
```

SQL Server:

```text
localhost,1433
```

### Useful API endpoints

Get the latest order book:

```text
GET http://localhost:5272/api/market/order-book
```

Get a buy quote:

```text
GET http://localhost:5272/api/market/quote?quantity=1
```

SignalR hub:

```text
http://localhost:5272/hubs/market
```

### View logs

```powershell
docker compose logs -f api
```

```powershell
docker compose logs -f frontend
```

```powershell
docker compose logs -f sqlserver
```

Stop the containers:

```powershell
docker compose down
```

The current Docker Compose configuration does not use a persistent SQL Server volume. Removing the SQL Server container removes its database data. Run the migration again after creating a fresh container.

> The SQL password in `docker-compose.yml` is intended only for local development. Do not use it in a production deployment.

## Run without Docker

### Requirements

- .NET 10 SDK
- Node.js 22 or later
- SQL Server LocalDB
- Git
- Internet access for Bitstamp

### Start SQL Server LocalDB

```powershell
sqllocaldb start MSSQLLocalDB
```

The local development connection string is stored in:

```text
BtcEurMarketDepth.Api/appsettings.Development.json
```

### Apply the database migration

From the `BtcEurMarketDepth` directory:

```powershell
dotnet ef database update `
  --project BtcEurMarketDepth.Infrastructure `
  --startup-project BtcEurMarketDepth.Api
```

### Start the backend

Open a terminal in the `BtcEurMarketDepth` directory:

```powershell
dotnet run --project BtcEurMarketDepth.Api
```

The backend runs at:

```text
http://localhost:5272
```

### Start the frontend

Open a second terminal:

```powershell
cd frontend
npm ci
npm run dev
```

The frontend runs at:

```text
http://localhost:5173
```

Open the application:

```text
http://localhost:5173
```

## Run Tests

### Backend tests

From the `BtcEurMarketDepth` directory:

```powershell
dotnet test BtcEurMarketDepth.slnx
```

### Frontend tests

From the `frontend` directory:

```powershell
npm run test:unit -- --run
```

### Frontend build

```powershell
npm run build
```

### Backend release build

From the `BtcEurMarketDepth` directory:

```powershell
dotnet build BtcEurMarketDepth.slnx --configuration Release
```

## Audit Log

Every acquired order-book snapshot is persisted to SQL Server before it is published to connected frontend clients.

The audit table is:

```text
dbo.order_book_snapshots
```

Example query:

```sql
USE BtcEurMarketDepth;
GO

SELECT TOP 20
    Id,
    Symbol,
    AcquiredAt,
    RecordedAt,
    BestBid,
    BestAsk
FROM dbo.order_book_snapshots
ORDER BY Id DESC;
```

`AcquiredAt` represents when the snapshot was acquired from Bitstamp.

`RecordedAt` represents when the snapshot was persisted by the application.

## Real-Time Updates

The backend polls Bitstamp approximately every five seconds.

For every successful snapshot:

1. The snapshot is acquired from Bitstamp.
2. The snapshot is saved to the audit database.
3. The latest snapshot is stored in memory.
4. The snapshot is broadcast through SignalR.
5. The frontend refreshes the order book, chart, and buy quote.

## Resilience

The Bitstamp HTTP client uses standard HTTP resilience policies, including:

- Retry attempts
- Retry delays with jitter
- Per-request timeout
- Total request timeout

These policies help the application handle temporary network failures without immediately stopping the polling process.

## Development Notes

The root `Directory.Packages.props` file centrally manages NuGet package versions for the .NET projects.

The frontend manages its dependencies separately through:

```text
frontend/package.json
frontend/package-lock.json
```

The project uses `.editorconfig` and frontend formatting rules to keep code style consistent.

## GitHub Actions

The GitHub Actions workflow is located at:

```text
.github/workflows/ci.yml
```

The workflow runs separate jobs for:

- Backend build
- Backend tests
- Frontend build
- Frontend tests

It runs automatically for pushes and pull requests targeting the configured main branch.

It can also be started manually from the repository's **Actions** tab.