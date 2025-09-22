# --------------------------
# Build stage
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копіюємо csproj окремо для кешу
COPY WarOfMachinesAPI/WarOfMachinesAPI.csproj WarOfMachinesAPI/
RUN dotnet restore WarOfMachinesAPI/WarOfMachinesAPI.csproj

# копіюємо решту
COPY . .
RUN dotnet publish WarOfMachinesAPI/WarOfMachinesAPI.csproj -c Release -o /app/out

# --------------------------
# Runtime stage
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Render задає PORT у середовищі → використовуємо його
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://0.0.0.0:${PORT}

COPY --from=build /app/out ./

EXPOSE 10000

ENTRYPOINT ["dotnet", "WarOfMachinesAPI.dll"]
