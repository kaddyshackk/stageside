# -- [ RESTORE ] ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS restore

WORKDIR /src

COPY ["StageSide.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["apps/scheduling-service/Scheduler.Domain/Scheduler.Domain.csproj", "apps/scheduling-service/Scheduler.Domain/"]
COPY ["apps/scheduling-service/Scheduler.Domain/packages.lock.json", "apps/scheduling-service/Scheduler.Domain/"]
COPY ["apps/scheduling-service/Scheduler.Data/Scheduler.Data.csproj", "apps/scheduling-service/Scheduler.Data/"]
COPY ["apps/scheduling-service/Scheduler.Data/packages.lock.json", "apps/scheduling-service/Scheduler.Data/"]
COPY ["apps/scheduling-service/Scheduler.Service/Scheduler.Service.csproj", "apps/scheduling-service/Scheduler.Service/"]
COPY ["apps/scheduling-service/Scheduler.Service/packages.lock.json", "apps/scheduling-service/Scheduler.Service/"]

RUN dotnet restore "StageSide.sln" --locked-mode

# -- [ BUILD ] ----
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

COPY . .

RUN dotnet publish apps/scheduling-service/Scheduler.Service/Scheduler.Service.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ PACKAGE API ] ----
FROM build AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Scheduler.Service.dll"]