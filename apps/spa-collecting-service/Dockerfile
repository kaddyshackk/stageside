# -- [ RESTORE ] ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore

WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["apps/spa-collecting-service/SpaCollector.sln", "./apps/spa-collecting-service/"]
COPY ["apps/spa-collecting-service/SpaCollector.Domain/SpaCollector.Domain.csproj", "./apps/spa-collecting-service/SpaCollector.Domain/"]
COPY ["apps/spa-collecting-service/SpaCollector.Domain/packages.lock.json", "./apps/spa-collecting-service/SpaCollector.Domain/"]
COPY ["apps/spa-collecting-service/SpaCollector.Data/SpaCollector.Data.csproj", "./apps/spa-collecting-service/SpaCollector.Data/"]
COPY ["apps/spa-collecting-service/SpaCollector.Data/packages.lock.json", "./apps/spa-collecting-service/SpaCollector.Data/"]
COPY ["apps/spa-collecting-service/SpaCollector.Service/SpaCollector.Service.csproj", "./apps/spa-collecting-service/SpaCollector.Service/"]
COPY ["apps/spa-collecting-service/SpaCollector.Service/packages.lock.json", "./apps/spa-collecting-service/SpaCollector.Service/"]
COPY ["packages/domain/Domain/Domain.csproj", "./packages/domain/Domain/"]
COPY ["packages/domain/Domain/packages.lock.json", "./packages/domain/Domain/"]
COPY ["packages/data/Data/Data.csproj", "./packages/data/Data/"]
COPY ["packages/data/Data/packages.lock.json", "./packages/data/Data/"]
COPY ["packages/contracts/Contracts/Contracts.csproj", "./packages/contracts/Contracts/"]
COPY ["packages/contracts/Contracts/packages.lock.json", "./packages/contracts/Contracts/"]
COPY ["packages/pipeline/Pipeline/Pipeline.csproj", "./packages/pipeline/Pipeline/"]
COPY ["packages/pipeline/Pipeline/packages.lock.json", "./packages/pipeline/Pipeline/"]
COPY ["packages/collection/Collection/Collection.csproj", "./packages/collection/Collection/"]
COPY ["packages/collection/Collection/packages.lock.json", "./packages/collection/Collection/"]
COPY ["packages/punchup/Punchup/Punchup.csproj", "./packages/punchup/Punchup/"]
COPY ["packages/punchup/Punchup/packages.lock.json", "./packages/punchup/Punchup/"]

RUN dotnet restore "./apps/spa-collecting-service/SpaCollector.sln" --locked-mode

# -- [ BUILD ] ----
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

COPY . .

RUN dotnet publish apps/spa-collecting-service/SpaCollector.Service/SpaCollector.Service.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ Install Playwright ] ----
FROM build AS playwright-install

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet build && \
    dotnet tool install --global Microsoft.Playwright.CLI && \
    /root/.dotnet/tools/playwright install --with-deps chromium

## -- [ Install Playwright Deps ] ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS playwright-install-deps

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Install system dependencies for Playwright browsers
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libdbus-1-3 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2 \
    libatspi2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# -- [ PACKAGE API ] ----
FROM playwright-install-deps AS final

WORKDIR /app

COPY --from=build /app/publish .
COPY --from=playwright-install /ms-playwright /ms-playwright

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "SpaCollector.Service.dll"]