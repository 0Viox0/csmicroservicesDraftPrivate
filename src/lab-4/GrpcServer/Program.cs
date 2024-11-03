using GrpcServer.Extensions;
using GrpcServer.Services;
using Task1.BackgroundServices;
using Task2.Extensions;
using Task3.Bll.Extensions;
using Task3.Dal.RepositoryExtensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Configuration.AddJsonFile(
    "externalServiceConnectionInfo.json",
    optional: false,
    reloadOnChange: true);

builder.Services
    .AddExternalServiceOptions(builder.Configuration)
    .AddDatabaseOptions(builder.Configuration)
    .AddCustomConfiguration(builder.Configuration, TimeSpan.FromSeconds(2))
    .AddOrderHistoryDataJsonSerializer()
    .AddRepositories()
    .AddBllServices()
    .AddMigrations()
    .AddNpgsqlDataSource()
    .AddPlMapper();

builder.Services.AddHostedService<ConfigurationUpdateBackgroundService>();
builder.Services.AddHostedService<MigrationBackgroundService>();

WebApplication app = builder.Build();

app.MapGrpcService<OrderGrpcService>();
app.MapGrpcService<ProductGrpcService>();
app.MapGrpcReflectionService();

app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();