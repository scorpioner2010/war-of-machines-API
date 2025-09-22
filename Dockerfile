# --------------------------
# Build stage
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копіюємо csproj окремо (кеш)
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

# Render дає змінну PORT; слухаємо саме її
ENV ASPNETCORE_URLS=http://+:${PORT}

COPY --from=build /app/out ./

# EXPOSE не обов'язковий для Render, але не завадить
EXPOSE 10000

ENTRYPOINT ["dotnet", "WarOfMachinesAPI.dll"]