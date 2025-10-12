#!/bin/bash

dotnet ef database update --context SchemaContext --project Data --startup-project Api
