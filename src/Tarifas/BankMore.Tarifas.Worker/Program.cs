using BankMore.Tarifas.Worker.Database;
using BankMore.Tarifas.Worker.Services;
using BankMore.Tarifas.Worker.Handlers;
using BankMore.Tarifas.Worker.Models;
using KafkaFlow;
using KafkaFlow.Serializer;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString not configured");

builder.Services.AddSingleton<IDbConnectionFactory>(
    new DbConnectionFactory(connectionString));

var initializer = new DatabaseInitializer(connectionString);
initializer.Initialize();

builder.Services.AddSingleton<ITarifaRepository, TarifaRepository>();

builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers([builder.Configuration["Kafka:BootstrapServers"]])
        .CreateTopicIfNotExists(builder.Configuration["Kafka:TopicTransferenciasRealizadas"], 1, 1)
        .CreateTopicIfNotExists(builder.Configuration["Kafka:TopicTarifacoesRealizadas"], 1, 1)
        .AddConsumer(consumer => consumer
            .Topic(builder.Configuration["Kafka:TopicTransferenciasRealizadas"])
            .WithGroupId(builder.Configuration["Kafka:ConsumerGroupId"])
            .WithBufferSize(100)
            .WithWorkersCount(10)
            .AddMiddlewares(middlewares => middlewares
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(handlers => handlers
                    .AddHandler<TransferCompletedHandler>()
                )
            )
        )
        .AddProducer(
            "tarifacoes-producer",
            producer => producer
                .DefaultTopic(builder.Configuration["Kafka:TopicTarifacoesRealizadas"])
                .AddMiddlewares(middlewares => middlewares
                    .AddSerializer<JsonCoreSerializer>()
                )
        )
    )
);

var host = builder.Build();

var bus = host.Services.CreateKafkaBus();
await bus.StartAsync();

await host.RunAsync();

await bus.StopAsync();
