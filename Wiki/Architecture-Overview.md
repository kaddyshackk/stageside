# Architecture Overview

## System Overview

ComedyPull API is a modular ASP.NET Core application that scrapes, processes, and serves comedy event data from various sources. The application follows Domain-Driven Design (DDD) principles with a clean, layered architecture.

## Layers

### 1. Api Layer
**Location**: `Api/`

**Responsibilities**:
- HTTP endpoint exposure
- Request/response handling
- Swagger documentation
- Authentication/Authorization
- Dependency injection configuration

**Key Files**:
- `Program.cs` - Application entry point, service registration
- `Controllers/` - REST API endpoints

### 2. Application Layer
**Location**: `Application/`

**Responsibilities**:
- Business logic orchestration
- Feature modules
- MediatR event handling
- Service interfaces

**Structure**:
```
Application/
├── Extensions/
│   └── ApplicationLayerExtensions.cs  # Service registration
├── Modules/
│   ├── DataProcessing/   # Pipeline orchestration
│   ├── Punchup/          # Punchup.live integration
│   └── DataSync/         # Scraping services
└── Interfaces/           # Cross-module interfaces
```

### 3. Domain Layer
**Location**: `Domain/`

**Responsibilities**:
- Core domain models
- Enums and value objects
- Business rules
- No external dependencies

**Key Entities**:
- `SourceRecord` - Scraped data record
- `Event` - Comedy event
- `Venue` - Event location
- `Comedian` - Performer

**Key Enums**:
- `ProcessingState` - Pipeline stages
- `DataSource` - Data source identifier
- `EntityType` - Entity classification
- `RecordType` - Record classification

### 4. Data Layer
**Location**: `Data/`

**Responsibilities**:
- Database access (EF Core)
- Repository implementations
- Database contexts
- Migrations

**Database Contexts**:
- `ComedyContext` - Main application data
- `ProcessingContext` - Pipeline processing data

### 5. Tests
**Location**: `Application.Tests/`

**Framework**: MSTest + FakeItEasy + FluentAssertions

---

## Architectural Patterns

### Modular Monolith

The application is organized into **feature modules**, each with clear boundaries:

```
Module Structure:
├── Processors/        # Module-specific processing logic
├── Services/          # Business services
├── Events/           # Domain events
├── Repositories/     # Data access interfaces
└── {Module}ModuleExtensions.cs  # DI registration
```

**Benefits**:
- Clear separation of concerns
- Independent feature development
- No circular dependencies between modules
- Easy to extract into microservices if needed

### Event-Driven Architecture

Uses **MediatR** for loosely-coupled communication:

```csharp
// Publisher
await mediator.Publish(new StateCompletedEvent(batchId, state));

// Handler
public class StateCompletedHandler : INotificationHandler<StateCompletedEvent>
{
    public async Task Handle(StateCompletedEvent notification, ...)
    {
        // React to event
    }
}
```

**Benefits**:
- Decoupled components
- Easy to add new handlers
- Testable event flows
- Supports multiple handlers per event

### Repository Pattern

Data access abstracted behind interfaces:

```csharp
// Interface in Application layer
public interface ISourceRecordRepository
{
    Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId);
}

// Implementation in Data layer
public class SourceRecordRepository : ISourceRecordRepository
{
    // EF Core implementation
}
```

**Benefits**:
- Testable business logic
- Swappable data stores
- Clear dependencies

### Dependency Injection

Heavy use of built-in .NET DI container:

```csharp
// Module registration
public static class PunchupModuleExtensions
{
    public static void AddPunchupModule(this IServiceCollection services)
    {
        services.AddScoped<ISubProcessor<DataSource>, PunchupTransformSubProcessor>();
    }
}

// Usage in Program.cs
builder.Services.AddPunchupModule();
builder.Services.AddDataProcessingModule();
```

---

## Module Overview

### DataProcessing Module

**Purpose**: Orchestrates the data processing pipeline

**Key Components**:
- `IStateProcessor` - Stage orchestrators
- `ISubProcessor<TKey>` - Source-specific logic
- `StateCompletedEvent` - Stage completion events
- `ProcessingStateMachine` - State transition rules

**Documentation**: [DataProcessing Module](./DataProcessing-Module.md)

### Punchup Module

**Purpose**: Punchup.live specific implementations

**Key Components**:
- `PunchupTransformSubProcessor` - Transform raw Punchup data
- `PunchupTicketsPageCollectorFactory` - Scraper factory
- `PunchupScrapeJob` - Quartz.NET scheduled job

### DataSync Module

**Purpose**: Data scraping and ingestion

**Key Components**:
- `IScraperFactory` - Creates source-specific scrapers
- `PlaywrightScraper` - Browser automation
- `SourceRecordIngestionService` - Batch insertion

---

## Data Flow

### High-Level Flow

```
1. Scraping (DataSync Module)
   ↓
   Web Scraper → Raw Data → SourceRecords (State: Ingested)

2. Processing Pipeline (DataProcessing Module)
   ↓
   Ingested → Transformed → DeDuped → Enriched → Linked → Completed

3. API Exposure (Api Layer)
   ↓
   Clients ← REST API ← Processed Data
```

### Processing Pipeline Detail

```
StateCompletedEvent Published
        ↓
StateCompletedHandler
        ↓
Find IStateProcessor (e.g., TransformStateProcessor)
        ↓
Load Batch from DB
        ↓
Group by Source
        ↓
Resolve ISubProcessor<DataSource> for each source
        ↓
Process Records (source-specific logic)
        ↓
Publish Next StateCompletedEvent
```

---

## Technology Stack

### Core Frameworks
- **.NET 8** - Runtime and framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **MediatR** - In-process messaging

### Database
- **PostgreSQL** - Primary database
- **Npgsql** - EF Core provider

### Background Processing
- **Quartz.NET** - Job scheduling

### Web Scraping
- **Playwright** - Browser automation
- **HtmlAgilityPack** - HTML parsing _(if used)_

### Testing
- **MSTest** - Test framework
- **FakeItEasy** - Mocking
- **FluentAssertions** - Assertion library

### Utilities
- **Newtonsoft.Json** - JSON serialization
- **Swagger/OpenAPI** - API documentation

---

## Configuration

### Application Settings

**Location**: `Api/Settings/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=comedypull;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ComedyPull": "Debug"
    }
  }
}
```

### Environment-Specific Settings

- `appsettings.Local.json` - Local development
- `appsettings.Development.json` - Development environment
- `appsettings.Staging.json` - Staging environment
- `appsettings.Production.json` - Production environment

### Service Registration

**Location**: `Application/Extensions/ApplicationLayerExtensions.cs`

```csharp
public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    services.AddPunchupModule();
    services.AddDataSyncModule(configuration);
    services.AddDataProcessingModule();
}
```

---

## Database Schema

### Processing Context

**SourceRecords Table**:
| Column | Type | Description |
|--------|------|-------------|
| Id | string | Primary key |
| BatchId | string | Processing batch identifier |
| Source | int (enum) | DataSource enum value |
| EntityType | int (enum) | Entity classification |
| RecordType | int (enum) | Record classification |
| State | int (enum) | Current processing state |
| Status | int (enum) | Processing status |
| RawData | text | Original scraped JSON |
| ProcessedData | text | Transformed JSON |
| ContentHash | string | De-dupe hash |
| CreatedAt | timestamp | Record creation time |
| UpdatedAt | timestamp | Last update time |
| IngestedAt | timestamp | Scraping time |

### Comedy Context

**Events, Venues, Comedians** tables (normalized schema)

---

## Design Principles

### SOLID Principles

1. **Single Responsibility**: Each processor handles one stage
2. **Open/Closed**: Extend via new sub-processors, no modification to core
3. **Liskov Substitution**: All `ISubProcessor<DataSource>` implementations interchangeable
4. **Interface Segregation**: Separate interfaces for state vs. sub-processors
5. **Dependency Inversion**: Depend on abstractions (`IStateProcessor`, not concrete types)

### Other Principles

- **DRY (Don't Repeat Yourself)**: Shared logic in base classes/helpers
- **YAGNI (You Aren't Gonna Need It)**: Features added when needed
- **Separation of Concerns**: Modules, layers, and processors have distinct responsibilities

---

## Development Workflow

### Adding a New Feature

1. **Identify Module**: Determine which module owns the feature
2. **Create Interfaces**: Define contracts in Application layer
3. **Implement Logic**: Add services/processors in appropriate module
4. **Register Services**: Update `{Module}ModuleExtensions.cs`
5. **Add Tests**: Create unit tests
6. **Update Docs**: Update relevant wiki pages

### Database Changes

1. **Update Entity**: Modify entity in Domain layer
2. **Create Migration**:
   ```bash
   dotnet ef migrations add MigrationName --context ProcessingContext --project Data --startup-project Api
   ```
3. **Apply Migration**:
   ```bash
   dotnet ef database update --context ProcessingContext --project Data --startup-project Api
   ```

### Running the Application

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run migrations
dotnet ef database update --context ComedyContext --project Data --startup-project Api
dotnet ef database update --context ProcessingContext --project Data --startup-project Api

# Run application
dotnet run --project Api

# Run tests
dotnet test
```

---

## Deployment

### Prerequisites

- .NET 8 Runtime
- PostgreSQL database
- Playwright browser binaries

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Host=...;Database=...;"
```

### Build & Deploy

```bash
# Publish
dotnet publish -c Release -o ./publish

# Run
cd publish
./Api
```

---

## Monitoring & Logging

### Logging

Uses built-in .NET logging with structured logging:

```csharp
logger.LogInformation("Processing {Count} records for batch {BatchId}", count, batchId);
```

**Log Levels**:
- `Trace` - Detailed diagnostic information
- `Debug` - Development debugging
- `Information` - General flow
- `Warning` - Unexpected but handled
- `Error` - Failures
- `Critical` - Application-breaking

### Monitoring

_(To be implemented)_
- Health checks
- Application Insights
- Performance counters

---

## Security Considerations

### Current Implementation

- Connection strings in configuration (not in source control)
- Parameterized SQL queries (EF Core)
- Input validation at API boundary

### Future Enhancements

- Authentication (JWT tokens)
- Authorization (role-based access)
- API rate limiting
- Secrets management (Azure Key Vault, etc.)

---

## Performance Considerations

### Current Optimizations

- Async/await throughout
- Batch processing (group operations)
- Database connection pooling (EF Core default)
- Indexed database columns

### Future Optimizations

- Caching (Redis)
- Parallel processing
- Database query optimization
- Response compression

---

## Related Documentation

- [Data Processing Module](./DataProcessing-Module.md)
- [Module Structure](./Module-Structure.md)
- [Development Guide](./Development-Guide.md) _(coming soon)_
