# Punchup Module

The Punchup module is a source-specific implementation that handles scraping, data collection, and transformation of comedy event data from punchup.live.

## Overview

The Punchup module demonstrates the source-specific plugin pattern used in ComedyPull. It integrates with both the DataSync and DataProcessing modules to:

1. **Scrape** comedy event data from punchup.live
2. **Collect** and queue raw data into the pipeline
3. **Transform** raw data into a standardized format for storage

## Architecture

The Punchup module follows the established modular architecture and integrates with the data pipeline:

```
┌─────────────────────────────────────────────────────────┐
│                    PunchupScrapeJob                     │
│              (Quartz Scheduled Job)                     │
│                                                         │
│  • Loads sitemap from punchup.live                      │
│  • Filters ticket page URLs                             │
│  • Orchestrates scraping process                        │
│  • Publishes StateCompletedEvent when done              │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│          PunchupTicketsPageCollector                    │
│              (IPageCollector)                           │
│                                                         │
│  • Scrapes comedian bio and event details               │
│  • Uses Playwright Page Object Model (POM)              │
│  • Enqueues SourceRecord to processing queue            │
└──────────────────┬──────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────┐
│         PunchupTransformSubProcessor                    │
│         (ISubProcessor<DataSource>)                     │
│                                                         │
│  • Transforms PunchupRecord → ProcessedComedian         │
│  • Generates URL-friendly slugs                         │
│  • Maps events to standardized format                   │
└─────────────────────────────────────────────────────────┘
```

## Key Components

### 1. PunchupScrapeJob

**Location**: `Application/Modules/Punchup/PunchupScrapeJob.cs`

A Quartz scheduled job that orchestrates the scraping process:

- Loads the sitemap from `https://www.punchup.live/sitemap.xml`
- Filters URLs using regex to find ticket pages matching pattern: `/[comedian-slug]/tickets`
- Supports limiting records for testing via `maxRecords` job parameter
- Uses Playwright to scrape pages in parallel
- Publishes `StateCompletedEvent(Ingested)` when finished to trigger the pipeline

**Key Dependencies**:
- `ISitemapLoader` - Loads sitemap XML
- `IPlaywrightScraperFactory` - Creates Playwright scraper instances
- `IPunchupTicketsPageCollectorFactory` - Creates page collectors
- `IMediator` - Publishes events to the pipeline

### 2. PunchupTicketsPageCollector

**Location**: `Application/Modules/Punchup/Collectors/PunchupTicketsPageCollector.cs`

Implements `IPageCollector` to scrape individual comedian ticket pages:

**Scraping Logic**:
1. Navigates to the ticket page URL
2. Waits for bio section to load
3. Extracts comedian name and bio
4. Checks if shows are available (handles "no shows" state)
5. If shows exist, clicks "See All" button if needed
6. Scrapes each show card for event details
7. Creates a `SourceRecord` with `PunchupRecord` data
8. Enqueues record to processing queue

**Data Collected**:
- Comedian name and bio
- Event details (date, time, venue, location, ticket link)

**Page Object Model Classes**:
- `TicketsPage` - Page-level locators (bio, show list)
- `ShowCard` - Individual show card locators (date, time, venue)

### 3. PunchupTransformSubProcessor

**Location**: `Application/Modules/Punchup/Processors/SubProcessors/PunchupTransformSubProcessor.cs`

A source-specific sub-processor that transforms raw Punchup data into the standardized format:

**Configuration**:
- **Key**: `DataSource.Punchup` - Registered for Punchup records only
- **FromState**: `ProcessingState.Ingested`
- **ToState**: `ProcessingState.Transformed`

**Transform Logic**:
1. Deserializes `PunchupRecord` from `SourceRecord.RawData`
2. Creates `ProcessedComedian` with:
   - Name and bio
   - URL-friendly slug (lowercase, hyphenated)
   - Mapped events with standardized fields
3. Serializes to `SourceRecord.ProcessedData`
4. Updates record state to `Transformed`

**Slug Generation**:
```csharp
name.ToLowerInvariant()
    .Replace(" ", "-")
    .Replace("'", "")
    .Replace(".", "")
```

### 4. Models

**Location**: `Application/Modules/Punchup/Models/`

#### PunchupRecord
Raw data model stored in `SourceRecord.RawData`:
```csharp
{
    string Name,
    string Bio,
    List<PunchupEvent> Events
}
```

#### PunchupEvent
Raw event data from punchup.live:
```csharp
{
    DateTimeOffset StartDateTime,
    string Location,
    string Venue,
    string TicketLink
}
```

#### ProcessedComedian
Standardized format stored in `SourceRecord.ProcessedData`:
```csharp
{
    string Name,
    string Slug,
    string Bio,
    List<ProcessedEvent> Events,
    string? SourceId,
    DateTimeOffset ProcessedAt
}
```

#### ProcessedEvent
Standardized event format:
```csharp
{
    string Title,              // Generated: "{Name} at {Venue}"
    DateTimeOffset StartDateTime,
    DateTimeOffset? EndDateTime,
    string Location,
    string Venue,
    string TicketLink,
    string? SourceId
}
```

### 5. Factories

**Location**: `Application/Modules/Punchup/Factories/`

#### IPunchupTicketsPageCollectorFactory
Factory interface for creating collectors with dependency injection support.

#### PunchupTicketsPageCollectorFactory
Concrete implementation that creates `PunchupTicketsPageCollector` instances with injected dependencies.

## Registration

**Location**: `Application/Modules/Punchup/PunchupModuleExtensions.cs`

The module registers its services using the `AddPunchupModule` extension method:

```csharp
public static void AddPunchupModule(this IServiceCollection services)
{
    services.AddScoped<IPunchupTicketsPageCollectorFactory,
                       PunchupTicketsPageCollectorFactory>();

    // Register Punchup-specific sub-processors
    services.AddScoped<ISubProcessor<DataSource>,
                       PunchupTransformSubProcessor>();
}
```

## Data Flow

1. **Ingestion** (DataSync Phase):
   ```
   PunchupScrapeJob
     → PunchupTicketsPageCollector
     → Queue<SourceRecord>
     → Database (State: Ingested)
   ```

2. **Transformation** (DataProcessing Phase):
   ```
   StateCompletedEvent(Ingested)
     → TransformStateProcessor
     → PunchupTransformSubProcessor
     → Database (State: Transformed)
   ```

3. **Next Stages**:
   - Enrichment (future)
   - Validation (future)
   - Published (loaded into domain entities)

## Configuration

**Location**: `Api/Settings/appsettings.json`

```json
{
  "DataSync": {
    "Punchup": {
      "SitemapUrl": "https://www.punchup.live/sitemap.xml",
      "UrlPattern": "^https?://(?:www\\.)?punchup\\.live/([^/]+)/tickets(?:/)?$"
    }
  }
}
```

## Testing

**Location**: `Application.Tests/Modules/Punchup/`

Test files include:
- `PunchupScrapeJobTests.cs` - Job orchestration tests
- `PunchupTicketsPageCollectorTests.cs` - Page scraping logic tests
- `Factories/PunchupTicketsPageCollectorFactoryTests.cs` - Factory tests

## Extension Points

To add a new source similar to Punchup:

1. Create a new module folder under `Application/Modules/`
2. Implement scraping jobs and collectors for data ingestion
3. Create source-specific models for raw and processed data
4. Implement `ISubProcessor<DataSource>` for transformation logic
5. Register services in a module extensions file
6. Add configuration to appsettings

See the [Data Processing Module](./DataProcessing-Module.md) documentation for details on the pipeline architecture.

## Related Documentation

- [Data Processing Module](./DataProcessing-Module.md) - Pipeline architecture
- [Architecture Overview](../Architecture-Overview.md) - System design