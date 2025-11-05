#!/bin/bash

# Check if .env file argument was passed
if [ -z "$1" ]; then
  ENV_FILE=".env"
else
  ENV_FILE="$1"
fi

# Resolve absolute path
if [[ "$ENV_FILE" != /* ]]; then
  ENV_FILE="$PWD/$ENV_FILE"
fi

# Check if file exists
if [ ! -f "$ENV_FILE" ]; then
  echo "Error: $ENV_FILE does not exist"
  exit 1
fi

# Load environment variables from file
set -a
source "$ENV_FILE"
set +a

# Shift to remove the first argument if provided
shift

dotnet ef "$@"
