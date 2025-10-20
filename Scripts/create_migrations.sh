#!/bin/bash

# Load local environment variables for host development (filter out comments and empty lines)
set -a
source <(grep -v '^#' .env.local | grep -v '^$' | sed 's/\r$//')
set +a

dotnet ef migrations add InitialCreate --context SchemaContext --project Data --startup-project Api
