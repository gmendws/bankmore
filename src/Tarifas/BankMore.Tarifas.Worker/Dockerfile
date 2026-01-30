# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY ["BankMore.sln", "./"]

# Copy project files
COPY ["src/Shared/BankMore.Shared/BankMore.Shared.csproj", "src/Shared/BankMore.Shared/"]
COPY ["src/Tarifas/BankMore.Tarifas.Worker/BankMore.Tarifas.Worker.csproj", "src/Tarifas/BankMore.Tarifas.Worker/"]

# Restore
RUN dotnet restore "src/Tarifas/BankMore.Tarifas.Worker/BankMore.Tarifas.Worker.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/Tarifas/BankMore.Tarifas.Worker"
RUN dotnet build "BankMore.Tarifas.Worker.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "BankMore.Tarifas.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Create data directory
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "BankMore.Tarifas.Worker.dll"]
