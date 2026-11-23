FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS build
WORKDIR /src
COPY RavApi/RavApi.csproj RavApi/
RUN dotnet restore RavApi/RavApi.csproj
COPY RavApi/ RavApi/
RUN dotnet publish RavApi/RavApi.csproj -c Release --no-restore -o /app/out /p:UseAppHost=false
COPY healthcheck/HealthProbe.csproj /src/healthcheck/
COPY healthcheck/Program.cs /src/healthcheck/
RUN dotnet publish /src/healthcheck/HealthProbe.csproj -c Release -o /health-probe /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS migrate
WORKDIR /src
COPY --from=build /src/RavApi /src/RavApi
RUN dotnet tool install --global dotnet-ef --version 10.0.15
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src/RavApi
ENTRYPOINT ["dotnet", "ef"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
COPY --from=build /health-probe /health-probe
RUN mkdir -p /var/lib/rav-staging/data-protection /run/credentials && chown -R $APP_UID:$APP_UID /var/lib/rav-staging
ENV ASPNETCORE_HTTP_PORTS=8080 ASPNETCORE_URLS=http://+:8080 Database__ApplyMigrationsOnStartup=false Database__RunSeedOnStartup=false Swagger__Enabled=false DataProtection__KeyDirectory=/var/lib/rav-staging/data-protection
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RavApi.dll"]
