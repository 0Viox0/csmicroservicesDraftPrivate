using GrpcServer.Extensions;
using GrpcServer.Interceptors;
using GrpcServer.Services;
using Task1.BackgroundServices;
using Task2.Extensions;
using Task3.Bll.Extensions;
using Task3.Dal.RepositoryExtensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options => options.Interceptors.Add<ErrorHandlingInterceptor>());
builder.Services.AddGrpcReflection();

builder.Services
    .AddExternalServiceOptions(builder.Configuration)
    .AddDatabaseOptions(builder.Configuration)
    .AddCustomConfiguration(builder.Configuration, TimeSpan.FromSeconds(2))
    .AddOrderHistoryDataJsonSerializer()
    .AddRepositories()
    .AddBllServices()
    .AddMigrations()
    .AddNpgsqlDataSource()
    .AddProductMapper();

builder.Services.AddHostedService<ConfigurationUpdateBackgroundService>();
builder.Services.AddHostedService<MigrationBackgroundService>();

WebApplication app = builder.Build();

app.MapGrpcService<OrderController>();
app.MapGrpcService<ProductController>();
app.MapGrpcReflectionService();

app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();