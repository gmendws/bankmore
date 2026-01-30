namespace BankMore.Transferencia.Infrastructure;

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

        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
        services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

        services.AddHttpClient<IContaCorrenteService, ContaCorrenteService>();

        services.AddScoped<IKafkaProducerService, KafkaProducerService>();

        return services;
    }
}
