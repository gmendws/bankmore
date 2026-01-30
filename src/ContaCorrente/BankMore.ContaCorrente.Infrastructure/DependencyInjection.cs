namespace BankMore.ContaCorrente.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Domain.Repositories;
using Application.Services;
using Repositories;
using Services;
using Database;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString não configurada");

        services.AddSingleton<IDbConnectionFactory>(
            new DbConnectionFactory(connectionString));

        var initializer = new DatabaseInitializer(connectionString);
        initializer.Initialize();

        services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
        services.AddScoped<IMovimentoRepository, MovimentoRepository>();
        services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

        services.AddSingleton<IJwtService, JwtService>();

        return services;
    }
}
