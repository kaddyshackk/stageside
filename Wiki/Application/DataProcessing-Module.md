# Data Processing Module

The Data Processing module orchestrates the movement of records through a multi-stage data pipeline using a state machine pattern and **medallion architecture** (Bronze → Silver → Gold).

## Overview

The module implements a **medallion architecture** with a **two-tier processor pattern**:

### Medallion Architecture
- **Bronze Layer**: Raw data from sources (`BronzeRecord`) - immutable source of truth
- **Silver Layer**: Transformed, flattened data (`SilverRecord`) - one entity per record
- **Gold Layer**: Production-ready data (`Comedian`, `Event`, `Venue`) - deduplicated, enriched entities

### Two-Tier Processor Pattern
- **Tier 1**: State Processors - Event-driven orchestrators that move batches between processing states
- **Tier 2**: Sub-Processors - Source-specific transformation logic (Transform stage only)

This design:
- Separates raw data from processed data
- Enables independent scaling of pipeline stages
- Supports batch processing and deduplication
- Eliminates circular dependencies between modules

## Table of Contents

- [Architecture](#architecture)
- [Medallion Layers](#medallion-layers)
- [State Machine](#state-machine)
- [Processing Flow](#processing-flow)
- [Key Components](#key-components)
- [Correlation Strategy](#correlation-strategy)
- [Adding New Processors](#adding-new-processors)

---

## Architecture

### Medallion Data Flow

```
┌──────────────────┐
│  Data Collection │
│   (Scrapers)     │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────────────────────┐
│                   BRONZE LAYER                        │
│  BronzeRecord - Raw, immutable source data           │
│  • One record per scraped page/API response          │
│  • Nested structures preserved                       │
│  • Content hash for deduplication                    │
└────────┬─────────────────────────────────────────────┘
         │
         │ Transform Stage
         │ (Flattens nested data)
         ▼
┌──────────────────────────────────────────────────────┐
│                   SILVER LAYER                        │
│  SilverRecord - Transformed, structured data         │
│  • One entity per record (Comedian OR Event OR Venue)│
│  • Flat structure with correlation slugs             │
│  • Enriched with computed fields                     │
└────────┬─────────────────────────────────────────────┘
         │
         │ Complete Stage
         │ (Deduplicates & persists)
         ▼
┌──────────────────────────────────────────────────────┐
│                    GOLD LAYER                         │
│  Comedian, Event, Venue, ComedianEvent               │
│  • Production-ready entities                         │
│  • Deduplicated by slug                              │
│  • Relationships established                         │
│  • Auditable (CreatedAt, UpdatedBy, etc.)           │
└──────────────────────────────────────────────────────┘
```

### Two-Tier Processor Pattern

```
┌─────────────────────────────────────────────────────────────┐
│                    StateCompletedEvent                      │
│                  (MediatR Notification)                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│               StateCompletedHandler                         │
│         Resolves IStateProcessor via DI                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│            IStateProcessor (Tier 1)                         │
│                                                             │
│  TransformStateProcessor:                                   │
│    1. Loads Batch + validates state                         │
│    2. Loads BronzeRecords by BatchId                        │
│    3. Resolves SubProcessor by Batch.SourceType            │
│    4. SubProcessor creates SilverRecords                    │
│    5. Updates Batch state & publishes event                 │
│                                                             │
│  CompleteStateProcessor:                                    │
│    1. Loads Batch + validates state                         │
│    2. Loads SilverRecords by BatchId                        │
│    3. Groups by EntityType                                  │
│    4. Batch processes each entity type:                     │
│       - Query existing Gold records by slug                 │
│       - Identify new vs existing                            │
│       - Bulk insert/update Gold records                     │
│    5. Updates Batch state & publishes event                 │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ (Transform stage only)
                       ▼
┌─────────────────────────────────────────────────────────────┐
│           ISubProcessor<DataSourceType> (Tier 2)            │
│                                                             │
│  PunchupTransformSubProcessor                               │
│  (Key = DataSourceType.PunchupTicketsPage)                  │
│                                                             │
│  1. Parses BronzeRecord.Data (nested JSON)                  │
│  2. Generates slugs for correlation                         │
│  3. Flattens into multiple SilverRecords:                   │
│     - 1 SilverRecord per Comedian (EntityType.Act)          │
│     - N SilverRecords per Venue (EntityType.Venue)          │
│     - M SilverRecords per Event (EntityType.Event)          │
│  4. Bulk creates SilverRecords                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Medallion Layers

### Bronze Layer - Raw Data

**Entity**: `BronzeRecord`

**Purpose**: Immutable storage of raw source data

**Fields**:
- `BatchId` - Groups records from same collection run
- `Data` - Raw JSON from source (nested structures preserved)
- `ContentHash` - For detecting duplicate scrapes
- `Status` - Processing status (Processing, Completed, Failed)

**Characteristics**:
- Immutable after creation
- One record per scraped page/API response
- Preserves original structure (nested objects)
- Never deleted (audit trail)

### Silver Layer - Transformed Data

**Entity**: `SilverRecord`

**Purpose**: Flattened, structured records ready for deduplication

**Fields**:
- `BatchId` - Links back to bronze layer
- `BronzeRecordId` - Source bronze record
- `EntityType` - Act, Event, or Venue
- `Data` - JSON of `Processed*` model (ProcessedAct, ProcessedEvent, ProcessedVenue)
- `Status` - Processing status
- `ContentHash` - For deduplication

**Characteristics**:
- One entity per record (flattened from nested bronze data)
- Includes correlation slugs for linking
- Enriched with computed fields (slugs, processed timestamps)
- Multiple silver records created from one bronze record

**Example Transformation**:
```
1 BronzeRecord (Punchup comedian page with nested events)
  ↓
3+ SilverRecords:
  - 1 SilverRecord { EntityType: Act, Data: ProcessedAct }
  - 1 SilverRecord { EntityType: Venue, Data: ProcessedVenue }
  - 1+ SilverRecord { EntityType: Event, Data: ProcessedEvent }
```

### Gold Layer - Production Data

**Entities**: `Comedian`, `Event`, `Venue`, `ComedianEvent`

**Purpose**: Deduplicated, production-ready entities with relationships

**Characteristics**:
- Unique by natural key (Slug)
- Fully auditable (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Relationships established (many-to-many via `ComedianEvent`)
- Traceable to source (Source, IngestedAt)
- Updateable (new scrapes update existing records)

---

## State Machine

### Processing States

Batches progress through the following states:

```
Ingested → Transformed → Completed
               ↓
            Failed
```

| State | Description | Layer Transition |
|-------|-------------|------------------|
| `Ingested` | Raw data scraped and stored as BronzeRecords | → Bronze |
| `Transformed` | Data flattened into SilverRecords with slugs | Bronze → Silver |
| `Completed` | SilverRecords persisted to Gold layer | Silver → Gold |
| `Failed` | Processing failed at some stage | - |

**Note**: Future states (DeDuped, Enriched, Linked) can be added between Transformed and Completed.

### State Machine Configuration

Defined in `ProcessingStateMachine.cs`:

```csharp
private static readonly Dictionary<ProcessingState, ProcessingState> ValidTransitions = new()
{
    { ProcessingState.Ingested, ProcessingState.Transformed },
    { ProcessingState.Transformed, ProcessingState.Completed }
};
```

**Key Methods**:
- `GetNextState(ProcessingState current)` - Returns next state or throws if terminal
- `CanTransition(ProcessingState from, ProcessingState to)` - Validates transition

---

## Processing Flow

### Complete Flow Example

#### 1. Data Ingestion (External to this module)
```csharp
// Scraper creates Batch
var batch = new Batch
{
    Source = DataSource.Punchup,
    SourceType = DataSourceType.PunchupTicketsPage,
    State = ProcessingState.Ingested
};

// Scraper creates BronzeRecords
var bronzeRecord = new BronzeRecord
{
    BatchId = batch.Id,
    Data = "{\"Name\":\"Joe List\",\"Events\":[...]}",  // Nested JSON
    ContentHash = ComputeHash(data)
};
```

#### 2. Transform Stage (Ingested → Transformed)

**TransformStateProcessor** orchestrates:

```csharp
public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
{
    // 1. Load and validate batch
    var batch = await repository.GetBatchById(batchId);
    if (batch.State != ProcessingState.Ingested) throw new InvalidBatchStateException();

    // 2. Load bronze records
    var bronzeRecords = await repository.GetBronzeRecordsByBatchId(batchId);

    // 3. Resolve SubProcessor by batch.SourceType
    var subProcessor = subProcessorResolver.Resolve(
        batch.SourceType,  // DataSourceType.PunchupTicketsPage
        FromState,         // Ingested
        ToState            // Transformed
    );

    // 4. SubProcessor creates SilverRecords
    await subProcessor.ProcessAsync(bronzeRecords, cancellationToken);

    // 5. Update batch state & publish event
    await repository.UpdateBatchStateById(batchId, ToState, cancellationToken);
    await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
}
```

**PunchupTransformSubProcessor** flattens data:

```csharp
public async Task ProcessAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken)
{
    var silverRecords = new List<SilverRecord>();

    foreach (var bronzeRecord in records)
    {
        var punchupData = JsonSerializer.Deserialize<PunchupRecord>(bronzeRecord.Data);
        var comedianSlug = GenerateSlug(punchupData.Name);  // "joe-list"

        // 1. Create SilverRecord for Comedian
        silverRecords.Add(new SilverRecord
        {
            BatchId = bronzeRecord.BatchId,
            BronzeRecordId = bronzeRecord.Id,
            EntityType = EntityType.Act,
            Data = JsonSerializer.Serialize(new ProcessedAct
            {
                Name = punchupData.Name,
                Slug = comedianSlug,
                Bio = punchupData.Bio
            })
        });

        // 2. Create SilverRecords for each unique Venue
        foreach (var venue in punchupData.Events.Select(e => e.Venue).Distinct())
        {
            silverRecords.Add(new SilverRecord
            {
                BatchId = bronzeRecord.BatchId,
                BronzeRecordId = bronzeRecord.Id,
                EntityType = EntityType.Venue,
                Data = JsonSerializer.Serialize(new ProcessedVenue
                {
                    Name = venue,
                    Slug = GenerateSlug(venue)  // "comedy-store"
                })
            });
        }

        // 3. Create SilverRecords for each Event
        foreach (var evt in punchupData.Events)
        {
            var venueSlug = GenerateSlug(evt.Venue);
            var eventSlug = $"{comedianSlug}-{venueSlug}-{evt.StartDateTime:yyyy-MM-dd}";

            silverRecords.Add(new SilverRecord
            {
                BatchId = bronzeRecord.BatchId,
                BronzeRecordId = bronzeRecord.Id,
                EntityType = EntityType.Event,
                Data = JsonSerializer.Serialize(new ProcessedEvent
                {
                    Title = $"{punchupData.Name} at {evt.Venue}",
                    Slug = eventSlug,
                    ComedianSlug = comedianSlug,  // For correlation
                    VenueSlug = venueSlug,        // For correlation
                    StartDateTime = evt.StartDateTime
                })
            });
        }
    }

    // Bulk create all SilverRecords
    await repository.CreateSilverRecordsAsync(silverRecords, cancellationToken);
}
```

#### 3. Complete Stage (Transformed → Completed)

**CompleteStateProcessor** persists to Gold layer:

```csharp
public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
{
    // 1. Load and validate batch
    var batch = await repository.GetBatchById(batchId);
    if (batch.State != ProcessingState.Transformed) throw new InvalidBatchStateException();

    // 2. Load silver records
    var records = await repository.GetSilverRecordsByBatchId(batchId);

    // 3. Group by EntityType and process each
    var groups = records.GroupBy(r => r.EntityType);
    foreach (var group in groups)
    {
        switch (group.Key)
        {
            case EntityType.Act:
                await ProcessActsAsync(group, cancellationToken);
                break;
            case EntityType.Venue:
                await ProcessVenuesAsync(group, cancellationToken);
                break;
            case EntityType.Event:
                await ProcessEventsAsync(group, cancellationToken);
                break;
        }
    }

    // 4. Update batch state & publish event
    await repository.UpdateBatchStateById(batchId, ToState, cancellationToken);
    await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
}

private async Task ProcessActsAsync(IEnumerable<SilverRecord> silverRecords, CancellationToken cancellationToken)
{
    // 1. Parse all ProcessedAct data
    var processedActs = silverRecords
        .Select(r => new {
            SilverRecord = r,
            ProcessedAct = JsonSerializer.Deserialize<ProcessedAct>(r.Data)
        })
        .ToList();

    // 2. Batch query for existing comedians by slug
    var slugs = processedActs.Select(x => x.ProcessedAct.Slug).Distinct().ToList();
    var existingComedians = await repository.GetComediansBySlugAsync(slugs);
    var existingBySlug = existingComedians.ToDictionary(c => c.Slug);

    // 3. Identify new vs existing
    var newComedians = new List<Comedian>();
    var updatedComedians = new List<Comedian>();

    foreach (var item in processedActs)
    {
        if (existingBySlug.TryGetValue(item.ProcessedAct.Slug, out var existing))
        {
            // Update if data changed
            if (existing.Bio != item.ProcessedAct.Bio)
            {
                existing.Bio = item.ProcessedAct.Bio;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                updatedComedians.Add(existing);
            }
        }
        else if (!newComedians.Any(nc => nc.Slug == item.ProcessedAct.Slug))
        {
            // Create new
            newComedians.Add(new Comedian
            {
                Name = item.ProcessedAct.Name,
                Slug = item.ProcessedAct.Slug,
                Bio = item.ProcessedAct.Bio ?? "",
                Source = batch.Source,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "System",
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = "System"
            });
        }

        item.SilverRecord.Status = ProcessingStatus.Completed;
    }

    // 4. Bulk insert new comedians
    if (newComedians.Any())
        await repository.AddComediansAsync(newComedians, cancellationToken);

    // 5. Bulk update existing comedians
    if (updatedComedians.Any())
        await repository.UpdateComediansAsync(updatedComedians, cancellationToken);

    // 6. Update SilverRecord statuses
    await repository.UpdateSilverRecordsAsync(silverRecords, cancellationToken);
}
```

---

## Key Components

### 1. Batch Entity

**Purpose**: Tracks a group of records through the pipeline

**Key Fields**:
- `Source` - DataSource enum (e.g., Punchup)
- `SourceType` - DataSourceType enum (e.g., PunchupTicketsPage)
- `State` - Current ProcessingState
- `Status` - Current ProcessingStatus

**Routing Logic**: SubProcessors are resolved by `Batch.SourceType`, not `DataSource`

**Why?** A single DataSource (e.g., Punchup) may have multiple page types:
- `PunchupTicketsPage` - Comedian pages with event listings
- `PunchupSearchPage` - Search results
- `PunchupVenuePage` - Venue-specific pages

Each requires different transformation logic.

### 2. State Processors

#### TransformStateProcessor

**Location**: `Application/Modules/DataProcessing/Steps/Transform/TransformStateProcessor.cs`

**Responsibilities**:
1. Load Batch and validate state
2. Load BronzeRecords by BatchId
3. Resolve SubProcessor by `Batch.SourceType`
4. Delegate to SubProcessor to create SilverRecords
5. Update Batch state and publish completion event

**Key Code**:
```csharp
// Resolve by SourceType (not DataSource!)
var subProcessor = subProcessorResolver.Resolve(
    batch.SourceType,  // DataSourceType enum
    FromState,
    ToState
);

await subProcessor.ProcessAsync(bronzeRecords, cancellationToken);
```

#### CompleteStateProcessor

**Location**: `Application/Modules/DataProcessing/Steps/Complete/CompleteStateProcessor.cs`

**Responsibilities**:
1. Load Batch and validate state
2. Load SilverRecords by BatchId
3. Group by EntityType
4. Process each entity type with batch operations:
   - Query existing Gold records by slug
   - Identify new vs existing
   - Bulk insert/update Gold records
5. Update Batch state and publish completion event

**No SubProcessors**: Complete stage doesn't use SubProcessors because:
- All SilverRecords follow the same schema regardless of source
- Processing is domain-driven (by EntityType), not source-driven
- Logic is generic: deserialize, deduplicate by slug, upsert

### 3. Sub-Processors (Transform Stage Only)

#### PunchupTransformSubProcessor

**Location**: `Application/Modules/Punchup/Processors/PunchupTransformSubProcessor.cs`

**Key**: `DataSourceType.PunchupTicketsPage`

**Responsibilities**:
1. Parse nested `PunchupRecord` JSON from BronzeRecord
2. Generate slugs for all entities
3. Flatten into separate SilverRecords:
   - 1 ProcessedAct (EntityType.Act)
   - N ProcessedVenue (EntityType.Venue) - deduplicated by venue name
   - M ProcessedEvent (EntityType.Event) - one per event
4. Populate correlation fields (ComedianSlug, VenueSlug)
5. Bulk create SilverRecords via repository

**Slug Generation**:
```csharp
// Comedian/Venue
private static string GenerateSlug(string name)
    => name.ToLowerInvariant()
           .Replace(" ", "-")
           .Replace("'", "")
           .Replace(".", "")
           .Replace(",", "")
           .Replace("&", "and");

// Event
private static string GenerateEventSlug(string comedianSlug, string venueSlug, DateTimeOffset date)
    => $"{comedianSlug}-{venueSlug}-{date:yyyy-MM-dd}";
```

### 4. Processed Models

These models represent the structure of `SilverRecord.Data`:

#### ProcessedAct
```csharp
public record ProcessedAct
{
    public string? Name { get; init; }
    public string? Slug { get; init; }           // For deduplication
    public string? Bio { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
}
```

#### ProcessedVenue
```csharp
public record ProcessedVenue
{
    public string? Name { get; init; }
    public string? Slug { get; init; }           // For deduplication
    public string? Location { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
}
```

#### ProcessedEvent
```csharp
public record ProcessedEvent
{
    public string? Title { get; init; }
    public string? Slug { get; init; }           // For deduplication
    public string? ComedianSlug { get; init; }   // For correlation
    public string? VenueSlug { get; init; }      // For correlation
    public DateTimeOffset? StartDateTime { get; init; }
    public DateTimeOffset? EndDateTime { get; init; }
    public string? TicketLink { get; init; }
}
```

---

## Correlation Strategy

### Problem
In the medallion architecture, each SilverRecord represents a single entity. How do we link Events to Comedians and Venues?

### Solution: Slug-Based Correlation

**1. Generate Slugs During Transform**

All entities get a slug that serves as a natural key:
- Comedian: Generated from name (`"Joe List"` → `"joe-list"`)
- Venue: Generated from name (`"Comedy Store"` → `"comedy-store"`)
- Event: Composite of comedian, venue, and date (`"joe-list-comedy-store-2024-10-15"`)

**2. Store Correlation Slugs in ProcessedEvent**

```csharp
var processedEvent = new ProcessedEvent
{
    Slug = "joe-list-comedy-store-2024-10-15",
    ComedianSlug = "joe-list",      // References ProcessedAct
    VenueSlug = "comedy-store",     // References ProcessedVenue
    // ...
};
```

**3. Lookup During Complete Stage**

```csharp
// In ProcessEventsAsync:
var comedianSlugs = processedEvents.Select(e => e.ComedianSlug).Distinct();
var venueSlugs = processedEvents.Select(e => e.VenueSlug).Distinct();

var comedians = await repository.GetComediansBySlugAsync(comedianSlugs);
var venues = await repository.GetVenuesBySlugAsync(venueSlugs);

// Map by slug for O(1) lookup
var comediansBySlug = comedians.ToDictionary(c => c.Slug);
var venuesBySlug = venues.ToDictionary(v => v.Slug);

// Create relationships
foreach (var eventData in processedEvents)
{
    var comedian = comediansBySlug[eventData.ComedianSlug];
    var venue = venuesBySlug[eventData.VenueSlug];

    var newEvent = new Event { /* ... */ VenueId = venue.Id };
    await repository.AddEventsAsync(new[] { newEvent });

    var comedianEvent = new ComedianEvent
    {
        ComedianId = comedian.Id,
        EventId = newEvent.Id
    };
    await repository.AddComedianEventsAsync(new[] { comedianEvent });
}
```

### Benefits
- ✅ No foreign key references needed in Silver layer
- ✅ Natural deduplication using slugs
- ✅ Handles missing references gracefully (logs warning, marks failed)
- ✅ Independent processing order (venues/comedians can be processed before or after events)

---

## Adding New Processors

### Adding a New State Transition

**Example**: Add DeDupe stage (Transformed → DeDuped)

1. **Update State Machine**
   ```csharp
   private static readonly Dictionary<ProcessingState, ProcessingState> ValidTransitions = new()
   {
       { ProcessingState.Ingested, ProcessingState.Transformed },
       { ProcessingState.Transformed, ProcessingState.DeDuped },     // Add this
       { ProcessingState.DeDuped, ProcessingState.Completed }
   };
   ```

2. **Create State Processor**
   ```csharp
   public class DeDupeStateProcessor : IStateProcessor
   {
       public ProcessingState FromState => ProcessingState.Transformed;
       public ProcessingState ToState => ProcessingState.DeDuped;

       public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
       {
           var batch = await repository.GetBatchById(batchId);
           var silverRecords = await repository.GetSilverRecordsByBatchId(batchId);

           // Group by ContentHash and mark duplicates
           var grouped = silverRecords.GroupBy(r => r.ContentHash);
           foreach (var group in grouped)
           {
               var first = group.First();
               first.Status = ProcessingStatus.Completed;

               foreach (var duplicate in group.Skip(1))
                   duplicate.Status = ProcessingStatus.Duplicate;
           }

           await repository.UpdateSilverRecordsAsync(silverRecords, cancellationToken);
           await repository.UpdateBatchStateById(batchId, ToState, cancellationToken);
           await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
       }
   }
   ```

3. **Register in DI**
   ```csharp
   services.AddScoped<IStateProcessor, TransformStateProcessor>();
   services.AddScoped<IStateProcessor, DeDupeStateProcessor>();  // Add this
   services.AddScoped<IStateProcessor, CompleteStateProcessor>();
   ```

### Adding a New Data Source

**Example**: Add Wikipedia as a data source

1. **Define SourceType**
   ```csharp
   public enum DataSourceType
   {
       PunchupTicketsPage,
       WikipediaComedianPage  // Add this
   }

   public enum DataSource
   {
       Punchup,
       Wikipedia  // Add this
   }
   ```

2. **Create SubProcessor**
   ```csharp
   public class WikipediaTransformSubProcessor : ISubProcessor<DataSourceType>
   {
       public DataSourceType? Key => DataSourceType.WikipediaComedianPage;
       public ProcessingState FromState => ProcessingState.Ingested;
       public ProcessingState ToState => ProcessingState.Transformed;

       public async Task ProcessAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken)
       {
           // Parse Wikipedia HTML, extract comedian info, create SilverRecords
       }
   }
   ```

3. **Register in DI**
   ```csharp
   // In Wikipedia module
   services.AddScoped<ISubProcessor<DataSourceType>, WikipediaTransformSubProcessor>();
   ```

**That's it!** The Complete stage automatically handles the new SilverRecords.

---

## Performance Considerations

### Batch Processing
- All queries use batch operations (load all records, query existing by list of slugs)
- Bulk inserts/updates reduce database roundtrips
- Efficient deduplication using in-memory dictionaries

### Processing Order
The Complete stage processes entity types in this order:
1. **Acts (Comedians)** - No dependencies
2. **Venues** - No dependencies
3. **Events** - Depends on Comedians and Venues existing

**Why?** Events reference Comedians and Venues. Processing them last ensures foreign keys are valid.

### Deduplication
- **Transform Stage**: Creates multiple SilverRecords from same BronzeRecord (expected behavior)
- **Complete Stage**: Deduplicates using slug lookups before inserting to Gold layer
- **Future**: Add DeDupe stage between Transform and Complete for advanced deduplication logic

---

## Testing

### Unit Tests

**State Machine Tests**: `Application.Tests/Modules/DataProcessing/ProcessingStateMachineTests.cs`

**Processor Tests**:
- `TransformStateProcessorTests.cs`
- `CompleteStateProcessorTests.cs`

### Testing Strategy

1. **Mock Repositories**: Use FakeItEasy for `ITransformStateRepository`, `ICompleteStateRepository`
2. **Test State Transitions**: Verify batch state updates correctly
3. **Test Flattening**: Verify 1 BronzeRecord → N SilverRecords
4. **Test Deduplication**: Verify slug-based duplicate detection
5. **Test Correlation**: Verify ComedianEvent relationships created correctly

---

## Troubleshooting

### Common Issues

**Issue**: "Comedian with slug X not found for event Y"
- **Cause**: Events processed before comedians, or comedian slug mismatch
- **Fix**: Ensure `ProcessActsAsync` runs before `ProcessEventsAsync` in Complete stage

**Issue**: "No processor found for SourceType X"
- **Cause**: SubProcessor not registered for that SourceType
- **Fix**: Register SubProcessor in DI with correct `Key` value

**Issue**: Duplicate Gold records created
- **Cause**: Slug generation inconsistency or missing slug lookup
- **Fix**: Verify `GetComediansBySlugAsync` is called before creating new records

### Debugging Tips

1. Check Batch.State to see where pipeline is stuck
2. Query SilverRecords to verify transformation happened correctly
3. Verify slugs match between ProcessedEvent and ProcessedAct/ProcessedVenue
4. Enable detailed logging for `CompleteStateProcessor`

---

## Future Enhancements

- **Transactions**: Wrap each entity type processing in a database transaction
- **Parallel Processing**: Process entity types concurrently
- **Incremental Updates**: Only process changed records
- **Advanced Deduplication**: Fuzzy matching for near-duplicates
- **Data Lineage**: Track which BronzeRecords contributed to each Gold record