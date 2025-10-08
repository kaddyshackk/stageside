# ComedyPull API Wiki

Welcome to the ComedyPull API documentation. This wiki provides comprehensive documentation for the application architecture, modules, and data processing pipeline.

## Table of Contents

- [Architecture Overview](./Architecture-Overview.md)
- [Data Processing Module](./DataProcessing-Module.md)
- [Module Structure](./Module-Structure.md)

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

## Getting Started

1. Review the [Architecture Overview](./Architecture-Overview.md) to understand the system design
2. Read the [Data Processing Module](./DataProcessing-Module.md) documentation to understand the pipeline
3. Explore specific module documentation as needed

## Contributing

When adding new features:
1. Follow the existing modular architecture pattern
2. Update relevant wiki documentation
3. Add unit tests for new functionality
4. Register new processors/services in the appropriate module extensions file
