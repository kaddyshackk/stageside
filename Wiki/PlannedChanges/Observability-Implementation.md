# Observability Implementation Plan

**Status**: Planned
**Priority**: High
**Estimated Effort**: 4-6 days
**Created**: 2025-10-11

## Overview

This document outlines the implementation plan to add comprehensive observability to the data pipeline by migrating from Quartz.NET to NCronJob and enhancing the domain model with execution tracking capabilities.

## Current State

### Existing Architecture
- **Quartz.NET**: Used only to trigger `PunchupScrapeJob` on demand via HTTP
- **MediatR**: Orchestrates pipeline via `StateCompletedEvent` and `StateCompletedHandler`
- **ProcessingStateMachine**: Enforces valid state transitions
- **Batch Entity**: Minimal tracking (State, Status only)
- **No Observability**: Cannot see execution history, duration, or metrics

### Problems
1. Quartz is underutilized (only job triggering, no scheduling)
2. No execution history or metrics tracking
3. No visibility into batch processing status
4. Pipeline observability requires external tooling

## Proposed Architecture

### Design Philosophy
**Separation of Concerns**:
- **NCronJob**: CRON scheduling (when to run)
- **MediatR**: Pipeline orchestration (what to run)
- **Batch Entity**: Execution tracking (what happened)

### Architecture Diagram
```
┌─────────────────────────────────────────┐
│   NCronJob (CRON Scheduler)            │
│   - Schedule jobs via attributes         │
│   - Trigger on CRON expressions         │
│   - Lightweight, no persistence         │
└─────────────────────────────────────────┘
              │
              │ Sends command
              ▼
┌─────────────────────────────────────────┐
│   MediatR (Pipeline Orchestration)      │
│   - StartBatchCommand                   │
│   - StateCompletedEvent                 │
│   - ProcessingStateMachine enforcement  │
│   - State transition logic              │
└─────────────────────────────────────────┘
              │
              │ Updates
              ▼
┌─────────────────────────────────────────┐
│   Batch Entity (Observability)          │
│   - Execution timestamps                │
│   - Stage-level metrics                 │
│   - Error tracking                      │
│   - Record counts                       │
└─────────────────────────────────────────┘
              │
              │ Queried by
              ▼
┌─────────────────────────────────────────┐
│   Monitoring APIs + Grafana             │
│   - Batch status endpoints              │
│   - Execution history                   │
│   - Dashboard visualizations            │
└─────────────────────────────────────────┘
```

## Implementation Plan

### Phase 1: Core Changes (2-3 days)

#### 1.1 Remove Quartz.NET
**Files to modify**:
- `Application/Application.csproj` - Remove Quartz packages
- `Application/Extensions/ApplicationLayerExtensions.cs` - Remove `AddQuartzServices()`
- `Api/Api.csproj` - Remove Quartz references if any

**Packages to remove**:
```xml
<PackageReference Include="Quartz" Version="3.15.0" />
<PackageReference Include="Quartz.Extensions.Hosting" Version="3.15.0" />
<PackageReference Include="Quartz.Serialization.Json" Version="3.15.0" />
```

**Database cleanup**:
- Run migration to drop `quartz.*` tables (optional, can keep for historical data)

#### 1.2 Add NCronJob
**Package to add**:
```xml
<PackageReference Include="LinkDotNet.NCronJob" Version="3.x" />
```

**Configuration** (`ApplicationLayerExtensions.cs`):
```csharp
public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddNCronJob(); // Add this
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // ... rest of configuration
}
```

#### 1.3 Enhance Batch Entity
**File**: `Domain/Modules/DataProcessing/Batch.cs`

**Add properties**:
```csharp
public record Batch : BaseEntity
{
    // Existing properties
    public DataSource Source { get; init; }
    public DataSourceType SourceType { get; init; }
    public ProcessingState State { get; set; } = ProcessingState.Ingested;
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Processing;

    // NEW: Execution tracking
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : null;

    // NEW: Record counts
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int FailedRecords { get; set; }

    // NEW: Error tracking
    public string? LastError { get; set; }
    public int RetryCount { get; set; }

    // NEW: Stage-level tracking (JSON column in PostgreSQL)
    public Dictionary<ProcessingState, StageExecution> StageExecutions { get; set; } = new();
}

public record StageExecution
{
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : null;
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}
```

**Database migration**:
```sql
ALTER TABLE batches ADD COLUMN started_at TIMESTAMP NOT NULL DEFAULT NOW();
ALTER TABLE batches ADD COLUMN completed_at TIMESTAMP;
ALTER TABLE batches ADD COLUMN total_records INTEGER DEFAULT 0;
ALTER TABLE batches ADD COLUMN processed_records INTEGER DEFAULT 0;
ALTER TABLE batches ADD COLUMN failed_records INTEGER DEFAULT 0;
ALTER TABLE batches ADD COLUMN last_error TEXT;
ALTER TABLE batches ADD COLUMN retry_count INTEGER DEFAULT 0;
ALTER TABLE batches ADD COLUMN stage_executions JSONB DEFAULT '{}'::jsonb;
```

#### 1.4 Create MediatR Commands
**New file**: `Application/Modules/Punchup/Commands/StartPunchupBatchCommand.cs`

```csharp
using MediatR;

namespace ComedyPull.Application.Modules.Punchup.Commands
{
    public record StartPunchupBatchCommand(int? MaxRecords = null) : IRequest;
}
```

**New file**: `Application/Modules/Punchup/Commands/StartPunchupBatchCommandHandler.cs`

```csharp
using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.Punchup.Collectors.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.Punchup.Commands
{
    public class StartPunchupBatchCommandHandler(
        ISitemapLoader sitemapLoader,
        IPlaywrightScraperFactory scraperFactory,
        IPunchupTicketsPageCollectorFactory collectorFactory,
        IBatchRepository batchRepository,
        IMediator mediator,
        ILogger<StartPunchupBatchCommandHandler> logger)
        : IRequestHandler<StartPunchupBatchCommand>
    {
        public async Task Handle(StartPunchupBatchCommand request, CancellationToken ct)
        {
            var batchId = Guid.NewGuid();
            logger.LogInformation("Starting Punchup batch {BatchId}", batchId);

            // Create batch entity for tracking
            var batch = new Batch
            {
                Id = batchId,
                Source = DataSource.Punchup,
                SourceType = DataSourceType.TicketsPage,
                State = ProcessingState.Ingested,
                Status = ProcessingStatus.Processing,
                StartedAt = DateTime.UtcNow
            };

            await batchRepository.CreateBatch(batch, ct);

            const string sitemapUrl = "https://www.punchup.live/sitemap.xml";
            var scraper = scraperFactory.CreateScraper();

            try
            {
                var urls = await sitemapLoader.LoadSitemapAsync(sitemapUrl);
                var regex = PunchupScrapeJob.GetTicketsPageUrlRegexForTesting();
                var matched = urls.Where(url => regex.IsMatch(url)).ToList();

                if (request.MaxRecords.HasValue && request.MaxRecords.Value > 0)
                {
                    matched = matched.Take(request.MaxRecords.Value).ToList();
                    logger.LogInformation("Limited to {MaxRecords} records", request.MaxRecords.Value);
                }

                // Update batch with record count
                batch.TotalRecords = matched.Count;
                await batchRepository.UpdateBatch(batch, ct);

                await scraper.InitializeAsync();
                if (matched.Count != 0)
                {
                    await scraper.RunAsync(matched, collectorFactory.CreateCollector);
                }

                // Trigger next pipeline stage
                await mediator.Publish(new StateCompletedEvent(batchId, ProcessingState.Ingested), ct);

                logger.LogInformation("Punchup batch {BatchId} ingestion completed", batchId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Punchup batch {BatchId} failed", batchId);

                // Update batch with error
                batch.Status = ProcessingStatus.Failed;
                batch.State = ProcessingState.Failed;
                batch.LastError = ex.Message;
                batch.CompletedAt = DateTime.UtcNow;
                await batchRepository.UpdateBatch(batch, ct);

                throw;
            }
            finally
            {
                scraper.Dispose();
            }
        }
    }
}
```

#### 1.5 Convert PunchupScrapeJob to NCronJob
**File**: `Application/Modules/Punchup/PunchupScrapeJob.cs`

**Before**:
```csharp
public class PunchupScrapeJob : IJob
{
    public static JobKey Key { get; } = new (nameof(PunchupScrapeJob));

    public async Task Execute(IJobExecutionContext context)
    {
        // ... 60 lines of scraping logic
    }
}
```

**After**:
```csharp
public class PunchupScrapeJob(
    IMediator mediator,
    ILogger<PunchupScrapeJob> logger)
{
    [NCronJob(scheduleExpression: "0 2 * * *")] // 2 AM daily, configurable via appsettings
    public async Task Execute(CancellationToken ct)
    {
        logger.LogInformation("CRON trigger: Starting Punchup batch");

        // Just trigger the command - all logic is in the handler
        await mediator.Send(new StartPunchupBatchCommand(), ct);
    }

    // Keep regex for backward compatibility
    [GeneratedRegex(@"^https?:\/\/(?:www\.)?punchup\.live\/([^\/]+)\/tickets(?:\/)?$")]
    public static partial Regex TicketsPageUrlRegex();
    public static Regex GetTicketsPageUrlRegexForTesting() => TicketsPageUrlRegex();
}
```

#### 1.6 Update JobController
**File**: `Api/Controllers/JobController.cs`

**Before**:
```csharp
[HttpPost("punchup")]
public async Task<IActionResult> TriggerPunchupScrape([FromQuery] int? maxRecords = null)
{
    var scheduler = await schedulerFactory.GetScheduler();
    // ... Quartz logic
}
```

**After**:
```csharp
[HttpPost("punchup")]
public async Task<IActionResult> TriggerPunchupScrape([FromQuery] int? maxRecords = null)
{
    try
    {
        // Directly trigger via MediatR (no Quartz needed)
        await mediator.Send(new StartPunchupBatchCommand(maxRecords));

        return Ok(new { message = "Punchup batch started successfully" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to start Punchup batch");
        return StatusCode(500, new { error = "Failed to start batch", details = ex.Message });
    }
}
```

#### 1.7 Update IBatchRepository
**File**: `Application/Modules/DataProcessing/Interfaces/IBatchRepository.cs`

**Add methods**:
```csharp
public interface IBatchRepository
{
    // Existing methods
    Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken);
    Task UpdateBatchStateById(string batchId, ProcessingState state, CancellationToken cancellationToken);
    Task UpdateBatchStatusById(string batchId, ProcessingStatus status, CancellationToken cancellationToken);

    // NEW methods
    Task CreateBatch(Batch batch, CancellationToken cancellationToken);
    Task UpdateBatch(Batch batch, CancellationToken cancellationToken);
    Task<IEnumerable<Batch>> GetBatchesByStatus(ProcessingStatus status, CancellationToken cancellationToken);
    Task<IEnumerable<Batch>> GetRecentBatches(int count, CancellationToken cancellationToken);
    Task<IEnumerable<Batch>> GetBatchesByDateRange(DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
}
```

#### 1.8 Update StateProcessors to Track Metrics
**Files**:
- `Application/Modules/DataProcessing/Steps/Transform/TransformStateProcessor.cs`
- `Application/Modules/DataProcessing/Steps/Complete/CompleteStateProcessor.cs`

**Add tracking** (example for TransformStateProcessor):
```csharp
public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken)
{
    logger.LogInformation("Starting {Stage} processing for batch {BatchId}", ToState, batchId);

    var stageStartTime = DateTime.UtcNow;

    try
    {
        // Load and validate batch
        var batch = await batchRepository.GetBatchById(batchId.ToString(), cancellationToken);

        // Track stage start
        batch.StageExecutions[ToState] = new StageExecution
        {
            StartedAt = stageStartTime
        };
        await batchRepository.UpdateBatch(batch, cancellationToken);

        // ... existing processing logic ...

        var bronzeRecords = await repository.GetBronzeRecordsByBatchId(batchId.ToString(), cancellationToken);
        var recordCount = bronzeRecords.Count();

        await subProcessor.ProcessAsync(bronzeRecords, cancellationToken);

        // Update batch state
        batch.State = ToState;
        batch.ProcessedRecords += recordCount;
        await batchRepository.UpdateBatchStateById(batchId.ToString(), ToState, cancellationToken);

        // Track stage completion
        batch.StageExecutions[ToState].CompletedAt = DateTime.UtcNow;
        batch.StageExecutions[ToState].RecordsProcessed = recordCount;
        await batchRepository.UpdateBatch(batch, cancellationToken);

        // Publish completion event
        await mediator.Publish(new StateCompletedEvent(batchId, ToState), cancellationToken);

        logger.LogInformation("Completed {Stage} processing for batch {BatchId}", ToState, batchId);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed {Stage} processing for batch {BatchId}", ToState, batchId);

        // Track stage failure
        var batch = await batchRepository.GetBatchById(batchId.ToString(), cancellationToken);
        batch.Status = ProcessingStatus.Failed;
        batch.LastError = ex.Message;
        batch.StageExecutions[ToState].CompletedAt = DateTime.UtcNow;
        batch.StageExecutions[ToState].ErrorMessage = ex.Message;
        await batchRepository.UpdateBatch(batch, cancellationToken);

        throw;
    }
}
```

### Phase 2: Monitoring APIs (1-2 days)

#### 2.1 Create BatchMonitoringController
**New file**: `Api/Controllers/BatchMonitoringController.cs`

```csharp
[ApiController]
[Route("api/batches")]
public class BatchMonitoringController(
    IBatchRepository batchRepository,
    ILogger<BatchMonitoringController> logger)
    : ControllerBase
{
    [HttpGet("{batchId:guid}")]
    public async Task<IActionResult> GetBatchStatus(Guid batchId, CancellationToken ct)
    {
        try
        {
            var batch = await batchRepository.GetBatchById(batchId.ToString(), ct);

            return Ok(new
            {
                batchId = batch.Id,
                source = batch.Source.ToString(),
                state = batch.State.ToString(),
                status = batch.Status.ToString(),
                startedAt = batch.StartedAt,
                completedAt = batch.CompletedAt,
                duration = batch.Duration,
                totalRecords = batch.TotalRecords,
                processedRecords = batch.ProcessedRecords,
                failedRecords = batch.FailedRecords,
                lastError = batch.LastError,
                stages = batch.StageExecutions.Select(s => new
                {
                    state = s.Key.ToString(),
                    startedAt = s.Value.StartedAt,
                    completedAt = s.Value.CompletedAt,
                    duration = s.Value.Duration,
                    recordsProcessed = s.Value.RecordsProcessed,
                    error = s.Value.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get batch status for {BatchId}", batchId);
            return NotFound(new { error = "Batch not found", batchId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBatches(
        [FromQuery] string? status = null,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        try
        {
            IEnumerable<Batch> batches;

            if (status != null && Enum.TryParse<ProcessingStatus>(status, true, out var statusEnum))
            {
                batches = await batchRepository.GetBatchesByStatus(statusEnum, ct);
            }
            else
            {
                batches = await batchRepository.GetRecentBatches(limit, ct);
            }

            return Ok(batches.Select(b => new
            {
                batchId = b.Id,
                source = b.Source.ToString(),
                state = b.State.ToString(),
                status = b.Status.ToString(),
                startedAt = b.StartedAt,
                completedAt = b.CompletedAt,
                duration = b.Duration,
                totalRecords = b.TotalRecords,
                processedRecords = b.ProcessedRecords,
                failedRecords = b.FailedRecords
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get batches");
            return StatusCode(500, new { error = "Failed to retrieve batches" });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveBatches(CancellationToken ct)
    {
        try
        {
            var batches = await batchRepository.GetBatchesByStatus(ProcessingStatus.Processing, ct);

            return Ok(batches.Select(b => new
            {
                batchId = b.Id,
                source = b.Source.ToString(),
                state = b.State.ToString(),
                startedAt = b.StartedAt,
                currentDuration = DateTime.UtcNow - b.StartedAt,
                totalRecords = b.TotalRecords,
                processedRecords = b.ProcessedRecords
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get active batches");
            return StatusCode(500, new { error = "Failed to retrieve active batches" });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetBatchStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var batches = await batchRepository.GetBatchesByDateRange(start, end, ct);

            var stats = new
            {
                totalBatches = batches.Count(),
                successfulBatches = batches.Count(b => b.Status == ProcessingStatus.Completed),
                failedBatches = batches.Count(b => b.Status == ProcessingStatus.Failed),
                averageDuration = batches
                    .Where(b => b.Duration.HasValue)
                    .Select(b => b.Duration!.Value.TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average(),
                totalRecordsProcessed = batches.Sum(b => b.ProcessedRecords),
                totalRecordsFailed = batches.Sum(b => b.FailedRecords)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get batch stats");
            return StatusCode(500, new { error = "Failed to retrieve batch statistics" });
        }
    }
}
```

#### 2.2 Create Batch Resume Service
**New file**: `Application/Modules/DataProcessing/Services/BatchRecoveryService.cs`

```csharp
using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Modules.DataProcessing.Services
{
    /// <summary>
    /// Background service that resumes incomplete batches after application restart.
    /// </summary>
    public class BatchRecoveryService(
        IBatchRepository batchRepository,
        IMediator mediator,
        ILogger<BatchRecoveryService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for application to be fully started
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            logger.LogInformation("Checking for incomplete batches to resume");

            try
            {
                var incompleteBatches = await batchRepository
                    .GetBatchesByStatus(ProcessingStatus.Processing, stoppingToken);

                foreach (var batch in incompleteBatches)
                {
                    logger.LogInformation(
                        "Resuming batch {BatchId} from state {State}",
                        batch.Id,
                        batch.State);

                    // Resume from last completed state
                    await mediator.Publish(
                        new StateCompletedEvent(batch.Id, batch.State),
                        stoppingToken);
                }

                logger.LogInformation("Batch recovery completed. Resumed {Count} batches",
                    incompleteBatches.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to recover incomplete batches");
            }
        }
    }
}
```

**Register service** in `ApplicationLayerExtensions.cs`:
```csharp
services.AddHostedService<BatchRecoveryService>();
```

### Phase 3: Visualization (Optional, 1-2 days)

#### 3.1 Grafana Setup
**Prerequisites**:
- Grafana installed (Docker or cloud)
- PostgreSQL data source configured

**Dashboard queries**:

**Panel 1: Batch Execution Timeline**
```sql
SELECT
    started_at as time,
    CASE
        WHEN status = 'Completed' THEN 1
        WHEN status = 'Failed' THEN -1
        ELSE 0
    END as value,
    source as metric
FROM batches
WHERE started_at > NOW() - INTERVAL '7 days'
ORDER BY started_at;
```

**Panel 2: Average Batch Duration by Source**
```sql
SELECT
    source,
    AVG(EXTRACT(EPOCH FROM (completed_at - started_at))) as avg_duration_seconds
FROM batches
WHERE completed_at IS NOT NULL
    AND started_at > NOW() - INTERVAL '30 days'
GROUP BY source;
```

**Panel 3: Success/Failure Rate**
```sql
SELECT
    DATE_TRUNC('day', started_at) as time,
    COUNT(CASE WHEN status = 'Completed' THEN 1 END) as successful,
    COUNT(CASE WHEN status = 'Failed' THEN 1 END) as failed
FROM batches
WHERE started_at > NOW() - INTERVAL '30 days'
GROUP BY DATE_TRUNC('day', started_at)
ORDER BY time;
```

**Panel 4: Records Processed Over Time**
```sql
SELECT
    started_at as time,
    processed_records as value
FROM batches
WHERE started_at > NOW() - INTERVAL '7 days'
ORDER BY started_at;
```

**Panel 5: Currently Running Batches**
```sql
SELECT
    id as batch_id,
    source,
    state,
    started_at,
    EXTRACT(EPOCH FROM (NOW() - started_at)) as duration_seconds,
    processed_records,
    total_records
FROM batches
WHERE status = 'Processing';
```

#### 3.2 Alert Configuration
**Alert 1: Batch Failure**
```sql
SELECT COUNT(*) as failed_count
FROM batches
WHERE status = 'Failed'
    AND started_at > NOW() - INTERVAL '1 hour';
```
Alert if: `failed_count > 0`

**Alert 2: Long-Running Batch**
```sql
SELECT COUNT(*) as stuck_count
FROM batches
WHERE status = 'Processing'
    AND started_at < NOW() - INTERVAL '2 hours';
```
Alert if: `stuck_count > 0`

**Alert 3: No Recent Batches**
```sql
SELECT COUNT(*) as recent_count
FROM batches
WHERE started_at > NOW() - INTERVAL '1 day';
```
Alert if: `recent_count = 0` (scheduled job not running)

## Configuration

### NCronJob CRON Expressions
Configure in `appsettings.json`:
```json
{
  "JobScheduling": {
    "PunchupScrape": {
      "CronExpression": "0 2 * * *",
      "TimeZone": "UTC",
      "Enabled": true
    }
  }
}
```

Update job to read from config:
```csharp
[NCronJob(scheduleExpression: "*/5 * * * *", TimeZoneInfo = "UTC")]
public async Task Execute(CancellationToken ct)
{
    // Use IOptions<JobSchedulingOptions> to read config
}
```

## Testing Strategy

### Unit Tests
1. **Command Handler Tests**:
   - Test `StartPunchupBatchCommandHandler` creates batch correctly
   - Test error handling updates batch status
   - Test record count tracking

2. **State Processor Tests**:
   - Verify stage execution tracking
   - Verify metrics are recorded
   - Test error scenarios

### Integration Tests
1. **End-to-End Pipeline**:
   - Trigger batch via API
   - Verify batch entity created
   - Verify state transitions
   - Verify final metrics recorded

2. **Recovery Service**:
   - Simulate restart with incomplete batches
   - Verify batches are resumed

### Manual Testing
1. Trigger job via `POST /api/jobs/punchup`
2. Check batch status via `GET /api/batches/{batchId}`
3. View active batches via `GET /api/batches/active`
4. Verify CRON scheduling works

## Migration Checklist

- [ ] Create `PlannedChanges` folder in Wiki
- [ ] Document current Quartz usage
- [ ] Backup Quartz tables (if preserving history)
- [ ] Add NCronJob package
- [ ] Remove Quartz packages
- [ ] Create database migration for Batch entity
- [ ] Implement enhanced Batch entity
- [ ] Create `StartPunchupBatchCommand` and handler
- [ ] Convert `PunchupScrapeJob` to NCronJob
- [ ] Update `JobController`
- [ ] Extend `IBatchRepository` interface
- [ ] Implement new repository methods
- [ ] Update `TransformStateProcessor` with tracking
- [ ] Update `CompleteStateProcessor` with tracking
- [ ] Create `BatchMonitoringController`
- [ ] Create `BatchRecoveryService`
- [ ] Register recovery service in DI
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Manual testing
- [ ] Update API documentation
- [ ] Configure CRON schedules
- [ ] Set up Grafana (optional)
- [ ] Deploy to production

## Rollback Plan

If issues arise during implementation:

1. **Keep Quartz tables** - Don't drop immediately
2. **Feature flag** - Add `UseNCronJob` configuration flag
3. **Parallel run** - Run both systems temporarily
4. **Data migration** - Script to populate Batch metrics from Quartz history

## Benefits

1. **Simplified Architecture**:
   - Remove underutilized Quartz dependency
   - Single orchestration model (MediatR)
   - Clear separation of concerns

2. **Better Observability**:
   - Execution history in domain model
   - Stage-level metrics
   - Error tracking
   - Performance monitoring

3. **Flexibility**:
   - Domain-driven observability
   - Easy to query via Entity Framework
   - No coupling to job framework
   - Can swap schedulers without changing domain

4. **Developer Experience**:
   - Simple attribute-based scheduling
   - Familiar ASP.NET patterns
   - Less boilerplate code
   - Easier testing

## Future Enhancements

### Phase 4: Advanced Features (Future)
- Add Serilog for structured logging
- Add OpenTelemetry for distributed tracing
- Implement distributed locking for clustering
- Add retry policies with exponential backoff
- Create custom Blazor dashboard
- Add webhook notifications on batch completion
- Implement batch priority queue
- Add pause/resume capabilities

## References

- [NCronJob Documentation](https://docs.ncronjob.dev/)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [Grafana PostgreSQL Data Source](https://grafana.com/docs/grafana/latest/datasources/postgres/)
- [ComedyPull Architecture Overview](../Architecture-Overview.md)
- [Data Processing Module](../Application/DataProcessing-Module.md)
