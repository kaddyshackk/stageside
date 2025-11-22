# -- [ RESTORE ] ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore

WORKDIR /src

COPY ["StageSide.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["apps/processing-service/Processor.sln", "./apps/processing-service/"]
COPY ["apps/processing-service/Processor.Domain/Processor.Domain.csproj", "./apps/processing-service/Processor.Domain/"]
COPY ["apps/processing-service/Processor.Domain/packages.lock.json", "./apps/processing-service/Processor.Domain/"]
COPY ["apps/processing-service/Processor.Data/Processor.Data.csproj", "./apps/processing-service/Processor.Data/"]
COPY ["apps/processing-service/Processor.Data/packages.lock.json", "./apps/processing-service/Processor.Data/"]
COPY ["apps/processing-service/Processor.Service/Processor.Service.csproj", "./apps/processing-service/Processor.Service/"]
COPY ["apps/processing-service/Processor.Service/packages.lock.json", "./apps/processing-service/Processor.Service/"]
COPY ["packages/domain/Domain/Domain.csproj", "./packages/domain/Domain/"]
COPY ["packages/domain/Domain/packages.lock.json", "./packages/domain/Domain/"]
COPY ["packages/data/Data/Data.csproj", "./packages/data/Data/"]
COPY ["packages/data/Data/packages.lock.json", "./packages/data/Data/"]
COPY ["packages/collection/Collection/Collection.csproj", "./packages/collection/Collection/"]
COPY ["packages/collection/Collection/packages.lock.json", "./packages/collection/Collection/"]
COPY ["packages/pipeline/Pipeline/Pipeline.csproj", "./packages/pipeline/Pipeline/"]
COPY ["packages/pipeline/Pipeline/packages.lock.json", "./packages/pipeline/Pipeline/"]
COPY ["packages/punchup/Punchup/Punchup.csproj", "./packages/punchup/Punchup/"]
COPY ["packages/punchup/Punchup/packages.lock.json", "./packages/punchup/Punchup/"]

RUN dotnet restore "./apps/processing-service/Processor.sln" --locked-mode

# -- [ BUILD ] ----
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

COPY . .

RUN dotnet publish apps/processing-service/Processor.Service/Processor.Service.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ PACKAGE API ] ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Processor.Service.dll"]