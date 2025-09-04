# ComedyPull API

A .NET 8 API for comedy event data synchronization and scraping.

## Prerequisites

- .NET 8 SDK
- Docker (optional)

## Quick Start

### Local Development

```bash
# Build solution
dotnet build

# Run API
dotnet run --project Api

# Run tests
dotnet test
```

### Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# API will be available at http://localhost:8080
```

## Project Structure

- `Api/` - Web API entry point
- `Application/` - Business logic and services  
- `Domain/` - Domain models and entities
- `Data/` - Data access layer
- `Application.Tests/` - Unit tests

## Environment

The API uses PostgreSQL for data storage. Connection strings can be configured in `appsettings.json` or via environment variables.