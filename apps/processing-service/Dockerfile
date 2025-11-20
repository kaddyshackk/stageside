# -- [ RESTORE ] ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS restore

WORKDIR /src

COPY ["StageSide.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["apps/processing-service/Processor.Domain/Processor.Domain.csproj", "apps/processing-service/Processor.Domain/"]
COPY ["apps/processing-service/Processor.Domain/packages.lock.json", "apps/processing-service/Processor.Domain/"]
COPY ["apps/processing-service/Processor.Data/Processor.Data.csproj", "apps/processing-service/Processor.Data/"]
COPY ["apps/processing-service/Processor.Data/packages.lock.json", "apps/processing-service/Processor.Data/"]
COPY ["apps/processing-service/Processor.Service/Processor.Service.csproj", "apps/processing-service/Processor.Service/"]
COPY ["apps/processing-service/Processor.Service/packages.lock.json", "apps/processing-service/Processor.Service/"]

RUN dotnet restore "StageSide.sln" --locked-mode

# -- [ BUILD ] ----
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

COPY . .

RUN dotnet publish apps/processing-service/Processor.Service/Processor.Service.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ PACKAGE API ] ----
FROM build AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Processor.Service.dll"]