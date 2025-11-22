

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "> Starting containers"
docker compose --env-file .env up -d --build

echo "> Waiting for postgres"
until docker compose ps postgres | grep -q "healthy"; do
  sleep 2
done

echo "> Applying migrations"

# scheduling-service
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/scheduling-service/Scheduler.Data --context SchedulingDbContext
# spa-collecting-service
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/spa-collecting-service/SpaCollector.Data --context SpaCollectingDbContext
# processing-service
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/processing-service/Processor.Data --context ComedyDbContext

