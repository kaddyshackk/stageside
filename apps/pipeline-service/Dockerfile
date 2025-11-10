# -- [ RESTORE ] ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore

WORKDIR /src

COPY ["ComedyPull.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Api/Api.csproj", "Api/"]
COPY ["Api/packages.lock.json", "Api/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Data/packages.lock.json", "Data/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Domain/packages.lock.json", "Domain/"]

RUN dotnet restore "ComedyPull.sln" --locked-mode

# -- [ BUILD ] ----
FROM restore AS build

ARG BUILD_CONFIGURATION=Release

COPY . .

RUN dotnet publish Api/Api.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ Install Playwright ] ----
FROM build AS playwright-install

ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet tool install --global Microsoft.Playwright.CLI && \
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

ENTRYPOINT ["dotnet", "Api.dll"]