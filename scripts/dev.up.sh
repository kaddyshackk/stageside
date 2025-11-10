

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "> Starting containers"
docker compose --env-file .env up -d --build

echo "> Waiting for postgres"
until docker compose ps postgres | grep -q "healthy"; do
  sleep 2
done

echo "> Applying migrations"
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/pipeline-service/Data --context ComedyDbContext
source "$SCRIPT_DIR/ef.sh" .env.local database update --project apps/pipeline-service/Data --context SchedulingDbContext
