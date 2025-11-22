#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "> Starting infrastructure services (postgres, rabbitmq, seq)"
docker compose --env-file .env --profile infrastructure up -d

echo "> Waiting for postgres to be healthy"
until docker compose ps postgres | grep -q "healthy"; do
  echo "  Waiting for postgres..."
  sleep 2
done

echo "> Waiting for rabbitmq to be healthy"
until docker compose ps rabbitmq | grep -q "healthy"; do
  echo "  Waiting for rabbitmq..."
  sleep 2
done

echo "> Infrastructure services are ready"
echo "> Applying migrations"

# scheduling-service
echo "  Applying scheduling-service migrations..."
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/scheduling-service/Scheduler.Data --context SchedulingDbContext

# spa-collecting-service
echo "  Applying spa-collecting-service migrations..."
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/spa-collecting-service/SpaCollector.Data --context SpaCollectingDbContext

# processing-service
echo "  Applying processing-service migrations..."
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/processing-service/Processor.Data --context ComedyDbContext

echo "> Migrations completed"
echo "> Starting microservices (scheduling, spa-collecting, processing)"
docker compose --env-file .env --profile services up -d --build

echo "> All services started successfully!"
echo "> Access points:"
echo "  - Scheduling Service: http://localhost:5281"
echo "  - SPA Collecting Service: http://localhost:5282"
echo "  - Processing Service: http://localhost:5283"
echo "  - Seq (Logging): http://localhost:5341"
echo "  - RabbitMQ Management: http://localhost:15672 (admin/password)"