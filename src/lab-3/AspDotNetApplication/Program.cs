using Microsoft.OpenApi.Models;
using Task1.BackgroundServices;
using Task1.CustomMiddleware;
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

string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Order management APi",
        Description = "An asp.net API for managing orders",
    });
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddHostedService<ConfigurationUpdateBackgroundService>();
builder.Services.AddHostedService<MigrationBackgroundService>();

builder.Services.AddScoped<ExceptionFormattingMiddleware>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionFormattingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();