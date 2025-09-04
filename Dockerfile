# -- [ Base Stage ] ----

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# -- [ BUILD STAGE ] ----

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ComedyPull.sln", "./"]
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Application.Tests/Application.Tests.csproj", "Application.Tests/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
RUN dotnet restore "ComedyPull.sln"
COPY . .
WORKDIR "/src/Api"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# -- [ PUBLISH STAGE ] ----

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Api"
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -- [ PRODUCTION BUILD STAGE ] ----
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]