# StageSide

**Never miss a show. Follow your favorite comedians.**

StageSide is a data platform that aggregates standup comedy events across the United States, helping comedy fans discover shows, follow their favorite performers, and stay connected to the live comedy scene.

## What is StageSide?

StageSide builds and maintains a comprehensive data lake of standup comedy events and performers in the US. By collecting information from various sources across the web, we create a centralized hub where comedy fans can:

- **Discover upcoming shows** in their area or nationwide
- **Follow comedians** they love and get notified about new performances
- **Find new favorites** by exploring the comedy landscape
- **Track venues** and discover comedy hotspots

## Why Does This Exist?

The standup comedy scene is fragmented. Event information is scattered across ticketing platforms, venue websites, social media, and comedian pages. Fans often miss shows because they didn't know their favorite comedian was in town, or they can't easily discover new acts performing nearby.

StageSide solves this by:
- Consolidating event data from multiple sources into one searchable database
- Making it easy to track comedians without checking dozens of websites
- Providing a complete picture of the comedy scene in any location
- Enabling fans to stay connected to live comedy

## Current State

**Status:** Active Development - Data Pipeline MVP

We currently have the core infrastructure in place:
- ✅ ELT (Extract, Load, Transform) pipeline architecture
- ✅ Data collection from initial sources (starting with Punchup.live)
- ✅ Event, comedian, and venue data models
- ✅ Automated data processing and normalization
- 🚧 Additional data sources integration (in progress)
- 🚧 User-facing applications (planned)

The system is successfully collecting and processing comedy event data, building the foundation for user-facing features.

## How It Works

StageSide operates as an automated data pipeline:

1. **Collection** - Automated services scan comedy event sources across the web
2. **Processing** - Raw data is cleaned, standardized, and organized
3. **Storage** - Events, comedians, and venues are stored in a structured data lake
4. **Access** - (Coming soon) APIs and applications provide access to the data

The system runs continuously, keeping event information fresh and up-to-date.

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- PostgreSQL (provided via Docker)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd stageside
   ```

2. **Start the infrastructure and services**
   ```bash
   ./scripts/dev.up.sh
   ```

   This script will:
   - Start PostgreSQL, RabbitMQ, and logging infrastructure
   - Apply database migrations
   - Launch all microservices

3. **Access the services**
   - Logging Dashboard (Seq): http://localhost:5341
   - RabbitMQ Management: http://localhost:15672

That's it! The system will begin collecting and processing comedy event data automatically.

### Stopping the System

```bash
docker compose down
```

## Project Structure

```
stageside/
├── apps/                      # Microservices
│   ├── scheduling-service/    # Coordinates data collection tasks
│   ├── spa-collecting-service/# Collects data from web sources
│   └── processing-service/    # Processes and stores data
├── packages/                  # Shared libraries
│   ├── domain/               # Core business models
│   ├── pipeline/             # Data transformation logic
│   └── punchup/              # Source-specific adapters
└── scripts/                  # Development and deployment scripts
```

## Roadmap

### Phase 1: Data Foundation (Current)
- ✅ Core pipeline architecture
- ✅ Initial data source integration (Punchup.live)
- 🚧 Additional major ticketing platforms
- 🚧 Data quality and deduplication improvements

### Phase 2: API & Access Layer
- Public API for querying events and comedians
- Real-time event updates
- Search and filtering capabilities

### Phase 3: User Applications
- Web application for browsing events
- Comedian following and notifications
- Location-based show discovery
- Mobile applications

### Phase 4: Community Features
- User reviews and ratings
- Show check-ins
- Comedian discovery recommendations

## Data Sources

StageSide currently collects data from:
- **Punchup.live** - Comedian tour dates and event information

We're actively working to integrate additional sources including major ticketing platforms, venue websites, and comedy-specific listing services.

## Technology Stack

- **.NET 9** - Core platform
- **PostgreSQL** - Data storage
- **RabbitMQ** - Message queue for pipeline coordination
- **Docker** - Containerization and deployment
- **Playwright** - Web scraping for dynamic content

## Documentation

For technical documentation, architecture details, and development guides, see the `/docs` directory _(coming soon)_.

## License

[License information to be added]

---

**Note:** StageSide is currently in active development. Features and capabilities are evolving rapidly. Star the repo to follow along with our progress!
