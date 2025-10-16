#!/bin/bash

# Load local environment variables for host development (filter out comments)
export $(cat .env.local | grep -v '^#' | xargs)

dotnet ef migrations add InitialCreate --context SchemaContext --project Data --startup-project Api
