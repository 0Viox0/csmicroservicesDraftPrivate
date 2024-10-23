using Task1.BackgroundServices;
using Task2.Extensions;
using Task3.Bll.Extensions;
using Task3.Dal.RepositoryExtensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
    .AddNpgsqlDataSource();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ConfigurationUpdateBackgroundService>();

// builder.Services.AddHostedService<TestBackgroundService>();
builder.Services.AddHostedService<MigrationBackgroundService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();