using Bll.Extensions;
using Dal.RepositoryExtensions;
using GrpcServer.BackgroundServices;
using GrpcServer.Extensions;
using GrpcServer.Interceptors;
using GrpcServer.Services;
using Task2.Extensions;

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
    .AddProductMapper()
    .AddKafkaToBll(builder.Configuration);

builder.Services.AddHostedService<ConfigurationUpdateBackgroundService>();
builder.Services.AddHostedService<MigrationBackgroundService>();

WebApplication app = builder.Build();

app.MapGrpcService<OrderController>();
app.MapGrpcService<ProductController>();
app.MapGrpcReflectionService();

app.Run();