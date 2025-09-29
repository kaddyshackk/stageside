#!/bin/bash

# Apply all Entity Framework migrations for ComedyPull API
echo "Applying migrations..."
dotnet ef database update --context ComedyContext --project Data

if [ $? -eq 0 ]; then
    echo "ComedyContext migrations applied successfully"
else
    echo "Failed to apply ComedyContext migrations"
    exit 1
fi

echo "Applying ProcessingContext migrations..."
dotnet ef database update --context ProcessingContext --project Data

if [ $? -eq 0 ]; then
    echo "ProcessingContext migrations applied successfully"
    echo "All migrations applied successfully!"
else
    echo "Failed to apply ProcessingContext migrations"
    exit 1
fi