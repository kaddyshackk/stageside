# Public Module Architecture

## Overview

The Public module serves user-facing queries for the comedy event directory (web + mobile apps). It implements CQRS with vertical slice architecture, maintaining strict separation from internal pipeline modules (DataProcessing, DataSync, Punchup) to enable future microservices extraction.

## Core Principles

1. **Clean Architecture**: Application layer independent of Data layer via interfaces
2. **CQRS**: Separate read models (queries) from write models (future commands)
3. **Vertical Slices**: Each feature contains all layers in dedicated folders
4. **Feature-Specific Queries**: Small, focused query interfaces instead of bloated repositories
5. **Module Isolation**: Public module cannot reference pipeline modules

## Layer Structure

```
Api/Modules/Public/
├── Events/
│   └── GetEventBySlug/
│       ├── GetEventBySlugEndpoint.cs      # Minimal API endpoint
│       └── EventDetailResponse.cs         # Response DTO
├── Comedians/
├── Venues/
└── PublicApiModule.cs                     # Endpoint registration

Application/Modules/Public/
├── Events/
│   └── Queries/
│       └── GetEventBySlug/
│           ├── GetEventBySlugQuery.cs     # MediatR request
│           ├── GetEventBySlugHandler.cs   # Query handler
│           ├── IGetEventBySlugQuery.cs    # Data access interface
│           └── EventDetailDto.cs          # Internal DTO
├── Comedians/
├── Venues/
└── PublicApplicationModule.cs             # Service registration

Data/Modules/Public/
├── Events/
│   ├── EventQueryContext.cs               # Read-only DbContext
│   ├── Queries/
│   │   └── GetEventBySlugQuery.cs         # IGetEventBySlugQuery implementation
│   └── Configurations/
│       └── EventConfiguration.cs          # EF Core mappings
├── Comedians/
├── Venues/
└── PublicDataModule.cs                    # DbContext registration
```

## CQRS Implementation

### Query Flow

```
HTTP Request (API)
    ↓
Minimal API Endpoint
    ↓
MediatR Query Object
    ↓
Query Handler (Application)
    ↓
Query Interface (Application)
    ↓
Query Implementation (Data)
    ↓
DbContext → Database
    ↓
Domain Entity
    ↓
DTO Mapping (Handler)
    ↓
HTTP Response (API)
```

### Example: GetEventBySlug

**Query Object** (Application layer):
```csharp
public record GetEventBySlugQuery(string Slug) : IRequest<EventDetailDto?>;
```

**Query Interface** (Application layer):
```csharp
public interface IGetEventBySlugQuery
{
    Task<Event?> ExecuteAsync(string slug, CancellationToken ct);
}
```

**Query Handler** (Application layer):
```csharp
public class GetEventBySlugHandler : IRequestHandler<GetEventBySlugQuery, EventDetailDto?>
{
    private readonly IGetEventBySlugQuery _query;
    private readonly ILogger<GetEventBySlugHandler> _logger;

    public async Task<EventDetailDto?> Handle(
        GetEventBySlugQuery request, 
        CancellationToken ct)
    {
        var @event = await _query.ExecuteAsync(request.Slug, ct);
        return @event is null ? null : MapToDto(@event);
    }
}
```

**Query Implementation** (Data layer):
```csharp
internal class GetEventBySlugQuery : IGetEventBySlugQuery
{
    private readonly IDbContextFactory<EventQueryContext> _contextFactory;

    public async Task<Event?> ExecuteAsync(string slug, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        return await context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.ComedianEvents).ThenInclude(ce => ce.Comedian)
            .FirstOrDefaultAsync(e => e.Slug == slug, ct);
    }
}
```

**Endpoint** (API layer):
```csharp
public static void Map(this IEndpointRouteBuilder app)
{
    app.MapGet("/{slug}", async (
        string slug,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var query = new GetEventBySlugQuery(slug);
        var result = await mediator.Send(query, ct);
        
        return result is null 
            ? Results.NotFound() 
            : Results.Ok(result);
    })
    .WithName("GetEventBySlug")
    .WithTags("Events")
    .Produces<EventDetailDto>()
    .Produces(StatusCodes.Status404NotFound);
}
```

## API Layer Design

### Minimal APIs with Feature Folders

Each feature gets its own folder containing:
- Endpoint definition
- Request/Response DTOs (if needed beyond route params)

### Endpoint Registration Pattern

```csharp
// Api/Modules/Public/Events/EventsEndpoints.cs
public static class EventsEndpoints
{
    public static void AddEventsEndpoints(this IEndpointRouteBuilder app)
    {
        GetEventBySlugEndpoint.Map(app);
        ListEventsEndpoint.Map(app);
        SearchEventsEndpoint.Map(app);
    }
}

// Program.cs
app.MapGroup("/api/events")
    .AddEventsEndpoints()
    .WithOpenApi();
```

## Data Access Strategy

### Feature-Specific Query Interfaces

Instead of monolithic repositories:

```csharp
// ✅ One interface per query
IGetEventBySlugQuery
IListEventsQuery
ISearchEventsQuery

// ❌ NOT this (repository bloat)
IEventRepository
{
    GetBySlug(...);
    List(...);
    Search(...);
    GetUpcoming(...);
    GetByVenue(...);
    // ... grows forever
}
```

### Read-Optimized Contexts

Each domain aggregate gets its own query context:

```csharp
public class EventQueryContext : DbContext
{
    public EventQueryContext(DbContextOptions<EventQueryContext> options) 
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Comedian> Comedians => Set<Comedian>();
    public DbSet<ComedianEvent> ComedianEvents => Set<ComedianEvent>();
}
```

## Module Boundaries

### Allowed Dependencies

```
Public → Domain (Event, Comedian, Venue entities)
Public → MediatR (IRequest, IRequestHandler)
Public → Logging (ILogger)
```

### Forbidden Dependencies

```
Public ✗ DataProcessing
Public ✗ DataSync
Public ✗ Punchup
Public ✗ Queue
```

This ensures clean extraction when splitting into microservices.

## Future Microservices Extraction

The Public module can become its own service with minimal changes:

1. Extract `Application/Modules/Public` → `PublicApi.Application`
2. Extract `Data/Modules/Public` → `PublicApi.Data`
3. Share `Domain/Modules/Common` (Event, Comedian, Venue) via NuGet or shared library
4. No code changes needed - only project references

Pipeline modules (DataProcessing, DataSync, Punchup) extract separately as "Pipeline Service."

## Testing Strategy

**Unit Tests** (Application layer):
- Mock `IGetEventBySlugQuery` interface
- Test handler logic and mapping

**Integration Tests** (Data layer):
- In-memory database or test container
- Test actual EF Core queries

**API Tests** (Api layer):
- WebApplicationFactory
- Test HTTP endpoints end-to-end

## Key Decisions

| Decision | Rationale |
|----------|-----------|
| Feature-specific query interfaces | Avoids repository bloat, keeps interfaces focused |
| Handlers in Application layer | Handlers are orchestration/workflow, not domain logic |
| Minimal APIs over Controllers | Less ceremony, better for vertical slices |
| MediatR for all queries | Consistent pattern, decouples API from Application |
| Separate contexts per aggregate | Read optimization, clear boundaries |
| `@event` for variable naming | Escapes C# keyword, clear and concise |

## Module Registration

```csharp
// Program.cs
builder.Services.AddApplicationServices(configuration);
builder.Services.AddDataServices(configuration);

// Application/Extensions/ApplicationLayerExtensions.cs
public static void AddApplicationServices(...)
{
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...));
    services.AddPublicApplicationModule();    // User-facing
    services.AddAdminApplicationModule();     // Internal observability
    services.AddDataProcessingModule();       // Pipeline (will extract)
    services.AddDataSyncModule();             // Pipeline (will extract)
}

// Data/Extensions/DataLayerExtensions.cs
public static void AddDataServices(...)
{
    services.AddPublicDataModule(configuration);
    services.AddAdminDataModule(configuration);
    services.AddDataProcessingServices(configuration);
    services.AddDataSyncServices(configuration);
}
```

## Next Features to Implement

1. **Events**: GetBySlug, List, Search
2. **Comedians**: GetBySlug, List, Search
3. **Venues**: GetBySlug, List, Search
4. **Admin**: ListBatches, GetBatchDetails, TriggerJobs
