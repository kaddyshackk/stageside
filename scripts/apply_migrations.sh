#!/bin/bash

dotnet ef database update --context TransformStateContext --project Data --startup-project Api
dotnet ef database update --context CompleteStateContext --project Data --startup-project Api
dotnet ef database update --context DataSyncContext --project Data --startup-project Api