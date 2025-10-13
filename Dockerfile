# -- [ BUILD STAGE ] ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore
COPY ["ComedyPull.sln", "./"]
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Application.Tests/Application.Tests.csproj", "Application.Tests/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
RUN dotnet restore "ComedyPull.sln"

# Copy source code and build
COPY . .
WORKDIR "/src/Api"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# -- [ PUBLISH STAGE ] ----
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Api"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ PRODUCTION BUILD STAGE ] ----
FROM mcr.microsoft.com/playwright/dotnet:v1.54.0-noble AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Browsers are pre-installed in the playwright/dotnet base image

# Expose ports
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "Api.dll"]