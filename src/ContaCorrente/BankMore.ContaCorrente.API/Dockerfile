# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY ["BankMore.sln", "./"]

# Copy all project files
COPY ["src/Shared/BankMore.Shared/BankMore.Shared.csproj", "src/Shared/BankMore.Shared/"]
COPY ["src/ContaCorrente/BankMore.ContaCorrente.Domain/BankMore.ContaCorrente.Domain.csproj", "src/ContaCorrente/BankMore.ContaCorrente.Domain/"]
COPY ["src/ContaCorrente/BankMore.ContaCorrente.Application/BankMore.ContaCorrente.Application.csproj", "src/ContaCorrente/BankMore.ContaCorrente.Application/"]
COPY ["src/ContaCorrente/BankMore.ContaCorrente.Infrastructure/BankMore.ContaCorrente.Infrastructure.csproj", "src/ContaCorrente/BankMore.ContaCorrente.Infrastructure/"]
COPY ["src/ContaCorrente/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj", "src/ContaCorrente/BankMore.ContaCorrente.API/"]

# Restore dependencies
RUN dotnet restore "src/ContaCorrente/BankMore.ContaCorrente.API/BankMore.ContaCorrente.API.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/ContaCorrente/BankMore.ContaCorrente.API"
RUN dotnet build "BankMore.ContaCorrente.API.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "BankMore.ContaCorrente.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

EXPOSE 8080

ENTRYPOINT ["dotnet", "BankMore.ContaCorrente.API.dll"]
