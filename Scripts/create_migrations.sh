#!/bin/bash

dotnet ef migrations add InitialCreate --context SchemaContext --project Data --startup-project Api
