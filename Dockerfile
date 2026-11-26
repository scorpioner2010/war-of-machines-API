# --------------------------
# Build stage
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /src

COPY RavApi/RavApi.csproj RavApi/
RUN dotnet restore RavApi/RavApi.csproj
COPY . .
RUN dotnet publish RavApi/RavApi.csproj -c Release --no-restore -o /app/out /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false && rm -f /app/out/appsettings.Development.json

COPY deploy/production/healthcheck/HealthProbe.csproj /src/healthcheck/
COPY deploy/production/healthcheck/Program.cs /src/healthcheck/
RUN dotnet publish /src/healthcheck/HealthProbe.csproj -c Release -o /health-probe /p:UseAppHost=false /p:DebugType=None /p:DebugSymbols=false

FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS migrate
WORKDIR /src
COPY --from=build /src/RavApi /src/RavApi
RUN dotnet tool install --global dotnet-ef --version 10.0.15
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src/RavApi
ENTRYPOINT ["dotnet", "ef"]

# --------------------------
# Runtime stage
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app

COPY --from=build /app/out ./
COPY --from=build /health-probe /health-probe
COPY deploy/staging/api/entrypoint.sh /app/entrypoint.sh
RUN chmod 0555 /app/entrypoint.sh && mkdir -p /data/protection && chown -R $APP_UID:$APP_UID /data

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Database__ApplyMigrationsOnStartup=false \
    Database__RunSeedOnStartup=false \
    Database__SeedDevelopmentFixtures=false \
    Swagger__Enabled=false \
    DataProtection__KeyDirectory=/data/protection
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["/app/entrypoint.sh"]
