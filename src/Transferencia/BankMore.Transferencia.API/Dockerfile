# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY ["BankMore.sln", "./"]

# Copy all project files
COPY ["src/Shared/BankMore.Shared/BankMore.Shared.csproj", "src/Shared/BankMore.Shared/"]
COPY ["src/Transferencia/BankMore.Transferencia.Domain/BankMore.Transferencia.Domain.csproj", "src/Transferencia/BankMore.Transferencia.Domain/"]
COPY ["src/Transferencia/BankMore.Transferencia.Application/BankMore.Transferencia.Application.csproj", "src/Transferencia/BankMore.Transferencia.Application/"]
COPY ["src/Transferencia/BankMore.Transferencia.Infrastructure/BankMore.Transferencia.Infrastructure.csproj", "src/Transferencia/BankMore.Transferencia.Infrastructure/"]
COPY ["src/Transferencia/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj", "src/Transferencia/BankMore.Transferencia.API/"]

# Restore dependencies
RUN dotnet restore "src/Transferencia/BankMore.Transferencia.API/BankMore.Transferencia.API.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/Transferencia/BankMore.Transferencia.API"
RUN dotnet build "BankMore.Transferencia.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "BankMore.Transferencia.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

EXPOSE 8080

ENTRYPOINT ["dotnet", "BankMore.Transferencia.API.dll"]
