# Data Processing Module

The Data Processing module orchestrates the movement of source records through a multi-stage data pipeline using a state machine pattern.

## Overview

The module uses a **two-tier processor architecture**:
- **Tier 1**: State Processors - Event-driven orchestrators that move batches between processing states
- **Tier 2**: Sub-Processors - Source-specific business logic implementations

This design eliminates circular dependencies while allowing source-specific modules (like Punchup) to extend the pipeline without modifying core processing logic.

## Table of Contents

- [Architecture](#architecture)
- [State Machine](#state-machine)
- [Processing Flow](#processing-flow)
- [Key Components](#key-components)
- [Adding New Processors](#adding-new-processors)
- [Replay Support](#replay-support)

---

## Architecture

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
│          e.g., TransformStateProcessor                      │
│                                                             │
│  1. Loads batch records from DB                             │
│  2. Groups by source (or other criteria)                    │
│  3. Resolves ISubProcessor for each group                   │
│  4. Delegates processing to sub-processors                  │
│  5. Publishes StateCompletedEvent for next stage            │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│           ISubProcessor<TKey> (Tier 2)                      │
│                                                             │
│  ┌──────────────────────────┐  ┌───────────────────────┐   │
│  │ GenericTransformSub-     │  │ PunchupTransformSub-  │   │
│  │ Processor (Key = null)   │  │ Processor             │   │
│  │ Fallback for all sources │  │ (Key = DataSource.    │   │
│  │                          │  │  Punchup)             │   │
│  └──────────────────────────┘  └───────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Why Two Tiers?

**Separation of Concerns:**
- **State Processors**: Handle pipeline orchestration, DB access, event publishing
- **Sub-Processors**: Contain only source-specific business logic

**No Circular Dependencies:**
- Core DataProcessing module registers state processors
- Source modules (e.g., Punchup) only register their sub-processors
- Modules never reference each other directly

**Single Database Query:**
- State processor loads batch once
- Distributes records to appropriate sub-processors
- Efficient and reduces DB load

---

## State Machine

### Processing States

Records progress through the following states:

```
Ingested → Transformed → DeDuped → Enriched → Linked → Completed
                                                   ↓
                                               Failed
```

| State | Description |
|-------|-------------|
| `Ingested` | Raw data scraped and stored in database |
| `Transformed` | Data parsed and transformed into structured format |
| `DeDuped` | Duplicate records identified and marked |
| `Enriched` | Additional data added (external APIs, calculations) |
| `Linked` | Relationships between entities established |
| `Completed` | Processing pipeline complete |
| `Failed` | Processing failed at some stage |

### State Machine Configuration

Defined in `ProcessingStateMachine.cs`:

```csharp
private static readonly Dictionary<ProcessingState, ProcessingState> ValidTransitions = new()
{
    { ProcessingState.Ingested, ProcessingState.Transformed },
    // Add more transitions as pipeline grows
};
```

**Key Methods:**
- `GetNextState(ProcessingState current)` - Returns next state or throws if terminal
- `CanTransition(ProcessingState from, ProcessingState to)` - Validates transition

---

## Processing Flow

### Complete Flow Example

1. **Data Ingestion** (External to this module)
   - Source data scraped and inserted into `SourceRecords` table
   - Records have `State = Ingested`, grouped by `BatchId`

2. **Manual Trigger** (or scheduled job)
   - Initial `StateCompletedEvent(batchId, ProcessingState.Ingested)` published

3. **Event Handler Activation**
   ```csharp
   StateCompletedHandler receives event
   → Calls ProcessingStateMachine.GetNextState(Ingested) → Transformed
   → Finds IStateProcessor where FromState=Ingested, ToState=Transformed
   → Invokes TransformStateProcessor.ProcessBatchAsync(batchId)
   ```

4. **State Processor Execution**
   ```csharp
   TransformStateProcessor:
   → Loads records: SELECT * FROM SourceRecords WHERE BatchId = @batchId
   → Groups by Source: records.GroupBy(r => r.Source)
   → For each group:
       → Resolve sub-processor: subProcessorResolver.Resolve(DataSource.Punchup, Ingested, Transformed)
       → Returns PunchupTransformSubProcessor (or GenericTransformSubProcessor if no match)
       → Calls subProcessor.ProcessAsync(records)
   → Publishes StateCompletedEvent(batchId, Transformed)
   ```

5. **Sub-Processor Execution**
   ```csharp
   PunchupTransformSubProcessor:
   → Receives IEnumerable<SourceRecord> for Punchup source
   → Parses RawData (JSON) into structured objects
   → Updates SourceRecord.ProcessedData with transformed JSON
   → Saves to database
   ```

6. **Next Stage Trigger**
   - `StateCompletedEvent(batchId, Transformed)` published
   - Cycle repeats for next transition (Transformed → DeDuped)

---

## Key Components

### 1. Interfaces

#### `IStateProcessor`
```csharp
public interface IStateProcessor
{
    ProcessingState FromState { get; }
    ProcessingState ToState { get; }
    Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken);
}
```

**Purpose**: Defines contract for pipeline stage orchestrators

**Implementations**:
- `TransformStateProcessor` (Ingested → Transformed)

#### `ISubProcessor<TKey>`
```csharp
public interface ISubProcessor<TKey> where TKey : struct
{
    TKey? Key { get; }  // null = generic/fallback
    ProcessingState FromState { get; }
    ProcessingState ToState { get; }
    Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken);
}
```

**Purpose**: Defines contract for source-specific business logic

**Key Parameter**: `TKey` - The type used to key/identify the processor (e.g., `DataSource`, `EntityType`)

**Implementations**:
- `GenericTransformSubProcessor` - Key = null (fallback)
- `PunchupTransformSubProcessor` - Key = DataSource.Punchup

---

### 2. State Processors

#### `TransformStateProcessor`

**Location**: `Application/Modules/DataProcessing/Processors/TransformStateProcessor.cs`

**Responsibilities**:
1. Load all records for a batch from database
2. Group records by source (DataSource enum)
3. Resolve appropriate sub-processor for each source
4. Delegate processing to sub-processors
5. Publish completion event

**Key Code**:
```csharp
public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
{
    // 1. Load records
    var records = await recordRepository.GetRecordsByBatchAsync(batchId.ToString());

    // 2. Group by source
    var recordsBySource = records.GroupBy(r => r.Source);

    // 3. Process each group
    foreach (var group in recordsBySource)
    {
        var subProcessor = subProcessorResolver.Resolve(group.Key, FromState, ToState);
        await subProcessor.ProcessAsync(group, cancellationToken);
    }

    // 4. Trigger next stage
    await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
}
```

---

### 3. Sub-Processors

#### `GenericTransformSubProcessor`

**Location**: `Application/Modules/DataProcessing/Processors/SubProcessors/`

**Purpose**: Fallback processor when no source-specific implementation exists

**Key Property**: `Key => null`

#### `PunchupTransformSubProcessor`

**Location**: `Application/Modules/Punchup/Processors/SubProcessors/`

**Purpose**: Punchup-specific transformation logic

**Key Property**: `Key => DataSource.Punchup`

**Example Implementation**:
```csharp
public async Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
{
    foreach (var record in records)
    {
        // Parse Punchup-specific JSON structure
        var rawData = JObject.Parse(record.RawData);

        // Transform to standardized format
        var transformed = new {
            EventName = rawData["title"]?.ToString(),
            VenueId = rawData["venue"]?["id"]?.ToString(),
            // ... more transformations
        };

        // Update record
        record.ProcessedData = JsonConvert.SerializeObject(transformed);
        record.State = ProcessingState.Transformed;
    }
}
```

---

### 4. Events

#### `StateCompletedEvent`

**Location**: `Application/Modules/DataProcessing/Events/StateCompletedEvent.cs`

```csharp
public record StateCompletedEvent(Guid BatchId, ProcessingState CompletedState) : INotification;
```

**Purpose**: Signals completion of a processing stage, triggers next stage

**Published By**: State processors after successful batch processing

**Handled By**: `StateCompletedHandler`

---

### 5. Event Handler

#### `StateCompletedHandler`

**Location**: `Application/Modules/DataProcessing/Events/StateCompletedHandler.cs`

**Responsibilities**:
1. Receive `StateCompletedEvent`
2. Determine next state via `ProcessingStateMachine`
3. Resolve appropriate `IStateProcessor` from DI
4. Invoke processor for next stage

**Key Logic**:
```csharp
public async Task Handle(StateCompletedEvent notification, CancellationToken cancellationToken)
{
    try
    {
        // Determine next state
        var nextState = ProcessingStateMachine.GetNextState(notification.CompletedState);

        // Find processor for transition
        var processor = stateProcessors.FirstOrDefault(p =>
            p.FromState == notification.CompletedState &&
            p.ToState == nextState);

        // Execute
        await processor.ProcessBatchAsync(notification.BatchId, cancellationToken);
    }
    catch (InvalidOperationException)
    {
        // Terminal state reached - processing complete
        logger.LogInformation("Batch {BatchId} processing completed", notification.BatchId);
    }
}
```

---

### 6. Sub-Processor Resolver

#### `ISubProcessorResolver` & `SubProcessorResolver`

**Location**: `Application/Modules/DataProcessing/Services/`

**Purpose**: Dynamically resolve the correct sub-processor based on key, state transition

**Resolution Logic**:
```csharp
public ISubProcessor<TKey> Resolve<TKey>(TKey key, ProcessingState fromState, ProcessingState toState)
    where TKey : struct
{
    var subProcessors = serviceProvider.GetService<IEnumerable<ISubProcessor<TKey>>>();

    // Try specific match
    var specific = subProcessors.FirstOrDefault(p =>
        Equals(p.Key, key) &&
        p.FromState == fromState &&
        p.ToState == toState);

    if (specific != null) return specific;

    // Fallback to generic (Key = null)
    var generic = subProcessors.FirstOrDefault(p =>
        p.Key == null &&
        p.FromState == fromState &&
        p.ToState == toState);

    return generic ?? throw new InvalidOperationException("No processor found");
}
```

---

### 7. Repository

#### `ISourceRecordRepository`

**Location**: `Application/Modules/DataProcessing/Repositories/Interfaces/`

**Methods**:
- `GetBatchSourceAsync(string batchId)` - Get the source for a batch
- `GetRecordsByBatchAsync(string batchId)` - Load all records in a batch

**Implementation**: `Data/Database/Repositories/SourceRecordRepository.cs`

---

## Adding New Processors

### Adding a New State Transition

**Example**: Add DeDupe stage (Transformed → DeDuped)

1. **Update State Machine**
   ```csharp
   // ProcessingStateMachine.cs
   private static readonly Dictionary<ProcessingState, ProcessingState> ValidTransitions = new()
   {
       { ProcessingState.Ingested, ProcessingState.Transformed },
       { ProcessingState.Transformed, ProcessingState.DeDuped }, // Add this
   };
   ```

2. **Create State Processor**
   ```csharp
   // Application/Modules/DataProcessing/Processors/DeDupeStateProcessor.cs
   public class DeDupeStateProcessor : IStateProcessor
   {
       public ProcessingState FromState => ProcessingState.Transformed;
       public ProcessingState ToState => ProcessingState.DeDuped;

       public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
       {
           var records = await recordRepository.GetRecordsByBatchAsync(batchId.ToString());
           var recordsBySource = records.GroupBy(r => r.Source);

           foreach (var group in recordsBySource)
           {
               var subProcessor = subProcessorResolver.Resolve(group.Key, FromState, ToState);
               await subProcessor.ProcessAsync(group, cancellationToken);
           }

           await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);
       }
   }
   ```

3. **Create Generic Sub-Processor**
   ```csharp
   // Application/Modules/DataProcessing/Processors/SubProcessors/GenericDeDupeSubProcessor.cs
   public class GenericDeDupeSubProcessor : ISubProcessor<DataSource>
   {
       public DataSource? Key => null; // Generic
       public ProcessingState FromState => ProcessingState.Transformed;
       public ProcessingState ToState => ProcessingState.DeDuped;

       public async Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
       {
           // Generic de-dupe logic using ContentHash
           var grouped = records.GroupBy(r => r.ContentHash);
           foreach (var group in grouped)
           {
               var first = group.First();
               first.State = ProcessingState.DeDuped;

               // Mark duplicates
               foreach (var duplicate in group.Skip(1))
               {
                   duplicate.Status = ProcessingStatus.Duplicate;
               }
           }
       }
   }
   ```

4. **Register in DI**
   ```csharp
   // Application/Modules/DataProcessing/DataProcessingModulesExtensions.cs
   public static void AddDataProcessingModule(this IServiceCollection services)
   {
       services.AddScoped<IStateProcessor, TransformStateProcessor>();
       services.AddScoped<IStateProcessor, DeDupeStateProcessor>(); // Add this

       services.AddScoped<ISubProcessor<DataSource>, GenericTransformSubProcessor>();
       services.AddScoped<ISubProcessor<DataSource>, GenericDeDupeSubProcessor>(); // Add this
   }
   ```

### Adding Source-Specific Sub-Processor

**Example**: Punchup-specific DeDupe logic

1. **Create in Source Module**
   ```csharp
   // Application/Modules/Punchup/Processors/SubProcessors/PunchupDeDupeSubProcessor.cs
   public class PunchupDeDupeSubProcessor : ISubProcessor<DataSource>
   {
       public DataSource? Key => DataSource.Punchup;
       public ProcessingState FromState => ProcessingState.Transformed;
       public ProcessingState ToState => ProcessingState.DeDuped;

       public async Task ProcessAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
       {
           // Punchup-specific de-dupe using event IDs from their API
           // ... custom logic
       }
   }
   ```

2. **Register in Source Module**
   ```csharp
   // Application/Modules/Punchup/PunchupModuleExtensions.cs
   public static void AddPunchupModule(this IServiceCollection services)
   {
       services.AddScoped<ISubProcessor<DataSource>, PunchupTransformSubProcessor>();
       services.AddScoped<ISubProcessor<DataSource>, PunchupDeDupeSubProcessor>(); // Add this
   }
   ```

**That's it!** No changes to DataProcessing module needed.

---

## Replay Support

The architecture naturally supports replaying specific stages:

### Manual Replay via State Processor

```csharp
// Inject the specific state processor
public class ReplayService(
    IEnumerable<IStateProcessor> stateProcessors)
{
    public async Task ReplayStage(Guid batchId, ProcessingState fromState, ProcessingState toState)
    {
        var processor = stateProcessors.FirstOrDefault(p =>
            p.FromState == fromState &&
            p.ToState == toState);

        if (processor == null)
            throw new InvalidOperationException($"No processor for {fromState} -> {toState}");

        await processor.ProcessBatchAsync(batchId, CancellationToken.None);
    }
}
```

### Replay Entire Pipeline

```csharp
public async Task ReplayFromState(Guid batchId, ProcessingState startState)
{
    // Publish initial event
    await mediator.Publish(new StateCompletedEvent(batchId, startState));

    // Pipeline continues automatically via event handlers
}
```

---

## Configuration

### Dependency Injection

**DataProcessing Module** (`DataProcessingModulesExtensions.cs`):
```csharp
services.AddScoped<ISubProcessorResolver, SubProcessorResolver>();
services.AddScoped<IStateProcessor, TransformStateProcessor>();
services.AddScoped<ISubProcessor<DataSource>, GenericTransformSubProcessor>();
services.AddScoped<INotificationHandler<StateCompletedEvent>, StateCompletedHandler>();
```

**Punchup Module** (`PunchupModuleExtensions.cs`):
```csharp
services.AddScoped<ISubProcessor<DataSource>, PunchupTransformSubProcessor>();
```

### Database

Records stored in `SourceRecords` table (ProcessingContext):
- `BatchId` - Groups records processed together
- `Source` - DataSource enum (Punchup, etc.)
- `State` - ProcessingState enum (current pipeline stage)
- `Status` - ProcessingStatus enum (Processing, Completed, Failed, Duplicate)
- `RawData` - Original scraped JSON
- `ProcessedData` - Transformed/enriched JSON

---

## Testing

### Unit Tests

**State Machine Tests**: `Application.Tests/Modules/DataProcessing/ProcessingStateMachineTests.cs`

**Event Handler Tests**: `Application.Tests/Modules/DataProcessing/Events/StateCompletedHandlerTests.cs`

### Testing Strategy

1. **Mock State Processors**: Use FakeItEasy to mock `IStateProcessor`
2. **Mock Sub-Processors**: Use FakeItEasy to mock `ISubProcessor<DataSource>`
3. **Test Event Flow**: Verify `StateCompletedHandler` calls correct processor
4. **Test Resolution Logic**: Verify `SubProcessorResolver` finds correct implementation

---

## Future Enhancements

### Planned Features

1. **Parallel Processing**: Process multiple sources concurrently within a batch
2. **Retry Logic**: Automatic retry for failed batches
3. **Dead Letter Queue**: Separate queue for repeatedly failing batches
4. **Progress Tracking**: Real-time progress updates via SignalR
5. **Pipeline Metrics**: Track processing times, success rates per stage

### Extensibility Points

- **New State Transitions**: Just update state machine and add processors
- **New Key Types**: Can key sub-processors on EntityType, RecordType, or custom criteria
- **Conditional Routing**: State processors can route based on complex logic
- **External Services**: Sub-processors can call external APIs for enrichment

---

## Troubleshooting

### Common Issues

**Issue**: "No processor found for transition X → Y"
- **Cause**: Missing state machine transition or missing state processor registration
- **Fix**: Add transition to `ProcessingStateMachine` and register `IStateProcessor` in DI

**Issue**: "No sub-processor found for key 'SourceName'"
- **Cause**: Source-specific sub-processor not registered, and no generic fallback exists
- **Fix**: Register generic sub-processor with `Key = null` as fallback

**Issue**: Batch processing hangs/never completes
- **Cause**: State processor not publishing `StateCompletedEvent` after processing
- **Fix**: Ensure `mediator.Publish(new StateCompletedEvent(...))` is called

### Debugging Tips

1. Enable detailed logging: Set log level to `Debug` for `ComedyPull.Application.Modules.DataProcessing`
2. Check event publication: Verify `StateCompletedEvent` is being published
3. Verify DI registration: Ensure all processors are registered correctly
4. Database state: Check `SourceRecords.State` column to see where records are stuck

---

## Related Documentation

- [Architecture Overview](./Architecture-Overview.md)
- [Module Structure](./Module-Structure.md)
- [Punchup Module](./Punchup-Module.md) _(coming soon)_
