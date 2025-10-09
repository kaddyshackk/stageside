#!/bin/bash

dotnet ef migrations add InitialCreateTransformState --context TransformStateContext --project Data --startup-project Api
dotnet ef migrations add InitialCreateCompleteState --context CompleteStateContext --project Data --startup-project Api
dotnet ef migrations add InitialCreateDataSync --context DataSyncContext --project Data --startup-project Api
