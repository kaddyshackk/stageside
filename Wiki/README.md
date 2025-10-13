# ComedyPull API Wiki

Welcome to the ComedyPull API documentation. This wiki provides comprehensive documentation for the application architecture, modules, and data processing pipeline.

## Table of Contents

### Core Documentation
- [Architecture Overview](./Architecture-Overview.md)

### Application Layer
- [Data Processing Module](./Application/DataProcessing-Module.md)
- [Punchup Module](./Application/Punchup-Module.md)

## Quick Links

### Core Concepts
- **Data Pipeline**: The system uses a state machine-driven pipeline to process scraped data through multiple stages
- **Modular Architecture**: The application is organized into feature modules (DataProcessing, Punchup, DataSync)
- **Event-Driven**: Uses MediatR for event-driven communication between pipeline stages

### Key Modules
- **DataProcessing**: Orchestrates the data processing pipeline using state processors and sub-processors
- **Punchup**: Source-specific implementation for punchup.live data
- **DataSync**: Handles data scraping and ingestion

## Project Structure

```
ComedyPull.Api/
├── Api/                    # ASP.NET Core Web API
├── Application/            # Application layer (business logic)
│   ├── Modules/
│   │   ├── DataProcessing/ # Pipeline orchestration
│   │   ├── Punchup/        # Punchup-specific logic
│   │   └── DataSync/       # Scraping & ingestion
├── Data/                   # Data access layer
├── Domain/                 # Domain models & enums
├── Application.Tests/      # Unit tests
└── Wiki/                   # Documentation
```

## Documentation Organization

The wiki is organized to mirror the .NET project structure:

```
wiki/
├── Architecture-Overview.md     # System-wide architecture
├── Application/                 # Application layer documentation
│   ├── DataProcessing-Module.md # Pipeline orchestration
│   └── Punchup-Module.md        # Punchup source module
├── Api/                         # API layer documentation (future)
├── Data/                        # Data layer documentation (future)
└── Domain/                      # Domain layer documentation (future)
```

## Getting Started

1. Review the [Architecture Overview](./Architecture-Overview.md) to understand the system design
2. Read the [Data Processing Module](./Application/DataProcessing-Module.md) to understand the pipeline
3. Explore the [Punchup Module](./Application/Punchup-Module.md) as an example of source-specific implementation
4. Use module documentation as templates when adding new features

## Contributing

When adding new features:
1. Follow the existing modular architecture pattern
2. Update relevant wiki documentation
3. Add unit tests for new functionality
4. Register new processors/services in the appropriate module extensions file
