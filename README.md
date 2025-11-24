# StageSide

**Live Event Data Platform | Active Development**

> A microservices-based data platform aggregating standup comedy events across the United States. Never miss a show from your favorite comedians.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](https://www.docker.com/)

[Architecture Overview](#architecture-highlights) | [Quick Start](#getting-started) | [Roadmap](#roadmap)

---

## Overview

StageSide is a data platform that aggregates standup comedy events across the United States, helping comedy fans discover shows, follow their favorite performers, and stay connected to the live comedy scene.

StageSide builds and maintains a comprehensive data lake of standup comedy events and performers in the US. By collecting information from various sources across the web, we create a centralized hub where comedy fans can:

- Discover upcoming shows in their area or nationwide
- Follow comedians they love and get notified about new performances
- Find new favorites by exploring the comedy landscape
- Track venues and discover comedy hotspots

---

## The Problem

The standup comedy scene is fragmented. Event information is scattered across dozens of ticketing platforms, venue websites, social media, and comedian pages. Fans often miss shows because they didn't know their favorite comedian was in town, or they can't easily discover new acts performing nearby.

## The Solution

StageSide solves this by:

- **Consolidating event data** from multiple sources into one searchable database
- **Automating data collection** through scheduled web scraping and API integration
- **Processing and normalizing** event information in real-time
- **Creating a centralized hub** that provides a complete picture of the comedy scene in any location
- **Enabling fan engagement** (coming soon) through comedian following and show notifications

## Why This Matters

This isn't just a coding exercise—it's solving a real pain point I experienced as a comedy fan. The technical challenges involved (distributed data collection, event deduplication, schema normalization across disparate sources, and real-time processing) mirror production systems at scale. Building this platform from the ground up has provided hands-on experience with enterprise-grade architecture patterns and data engineering practices.

---

## Architecture Highlights

- **Event-driven microservices** communicating via RabbitMQ for asynchronous processing
- **Automated data collection** using Playwright for dynamic content extraction
- **ELT pipeline** with scheduled collection, processing, and storage workflows
- **Containerized deployment** with Docker Compose for local development and production
- **Observability** with structured logging (Seq) for monitoring and debugging
- **Domain-driven design** with shared packages for business logic and data models

### System Components

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

---

## Current Status

**Status: Active Development - Data Pipeline MVP**

We currently have the core infrastructure in place:

- ✅ ELT (Extract, Load, Transform) pipeline architecture
- ✅ Data collection from initial sources (starting with Punchup.live)
- ✅ Event, comedian, and venue data models
- ✅ Automated data processing and normalization
- 🚧 Additional data sources integration (in progress)
- 🚧 User-facing applications (planned)

The system is successfully collecting and processing comedy event data, building the foundation for user-facing features.

---

## How It Works

StageSide operates as an automated data pipeline:

1. **Collection** - Automated services scan comedy event sources across the web on scheduled intervals
2. **Processing** - Raw data is cleaned, standardized, and organized through transformation workflows
3. **Storage** - Events, comedians, and venues are stored in a structured PostgreSQL database
4. **Access** - (Coming soon) REST APIs and web applications provide access to the data

The system runs continuously, keeping event information fresh and up-to-date.

---

## Technology Stack

**Backend & Services**
- .NET 9 - Core platform and microservices
- C# - Primary programming language
- PostgreSQL - Relational database for structured data storage
- RabbitMQ - Message broker for inter-service communication

**Infrastructure & DevOps**
- Docker & Docker Compose - Containerization and orchestration
- Playwright - Web scraping for dynamic content
- Seq - Structured logging and observability

**Architecture Patterns**
- Microservices architecture
- Event-driven design
- Domain-driven design (DDD)
- ELT data pipeline

---

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/get-started) and Docker Compose
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (for local development)
- PostgreSQL (provided via Docker)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/kaddyshackk/stageside.git
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
   - Logging Dashboard (Seq): [http://localhost:5341](http://localhost:5341)
   - RabbitMQ Management: [http://localhost:15672](http://localhost:15672)

That's it! The system will begin collecting and processing comedy event data automatically.

### Stopping the System

```bash
docker compose down
```

---

## Roadmap

### Phase 1: Core Pipeline (Current)
- ✅ Core pipeline architecture
- ✅ Initial data source integration (Punchup.live)
- 🚧 Additional major ticketing platforms
- 🚧 Data quality and deduplication improvements

### Phase 2: API Layer
- Public REST API for querying events and comedians
- Real-time event updates via webhooks
- Search and filtering capabilities
- Rate limiting and authentication

### Phase 3: User-Facing Applications
- Web application for browsing events
- Comedian following and notifications
- Location-based show discovery
- User authentication and profiles

### Phase 4: Community Features
- Mobile applications (iOS/Android)
- User reviews and ratings
- Show check-ins and social features
- Comedian discovery recommendations

---

## Data Sources

StageSide currently collects data from:

- **Punchup.live** - Comedian tour dates and event information

We're actively working to integrate additional sources including major ticketing platforms, venue websites, and comedy-specific listing services.

---

## Documentation

For technical documentation, architecture details, and development guides, see the `/docs` directory (coming soon).

---

## Contributing

This is currently a personal project in active development. Contributions, suggestions, and feedback are welcome! Feel free to open an issue or reach out directly.

---

## License

[License information to be added]

---

## Contact

**Joe Kadlic**
- Email: joekadlic@outlook.com
- LinkedIn: [linkedin.com/in/joekadlic](https://linkedin.com/in/joekadlic)
- GitHub: [@kaddyshackk](https://github.com/kaddyshackk)

---

**Note:** StageSide is currently in active development. Features and capabilities are evolving rapidly. Star the repo to follow along with our progress!
