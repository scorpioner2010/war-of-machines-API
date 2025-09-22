# --------------------------
# Build stage
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копіюємо всі файли
COPY . .

# Переходимо до каталогу з проєктом
WORKDIR /app/WarOfMachinesAPI

# Відновлення залежностей і публікація
RUN dotnet restore WarOfMachinesAPI.csproj
RUN dotnet publish WarOfMachinesAPI.csproj -c Release -o /app/out

# --------------------------
# Runtime stage
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out ./

# Важливо для Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "WarOfMachinesAPI.dll"]