using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Task1.ConfigurationModels;
using Task3.Bll.Mappers;
using Task3.Bll.Services;
using Task3.Dal.Migrations;
using Task3.Dal.Models.Enums;

namespace Task3.Bll.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBllServices(this IServiceCollection services)
    {
        services.AddScoped<OrderService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderHistoryMapper>();

        return services;
    }

    public static IServiceCollection AddDatabaseOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        return services;
    }

    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(sp =>
                    sp.GetRequiredService<IOptions<DatabaseSettings>>().Value.ConnectionString)
                .ScanIn(typeof(CreateProductTable).Assembly).For.Migrations());

        return services;
    }

    public static IServiceCollection AddNpgsqlDataSource(this IServiceCollection services)
    {
        services
            .AddSingleton(sp =>
            {
                DatabaseSettings dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                var sourceBuilder = new NpgsqlDataSourceBuilder(dbSettings.ConnectionString);

                sourceBuilder.MapEnum<OrderHistoryItemKind>();
                sourceBuilder.MapEnum<OrderState>();

                return sourceBuilder.Build();
            });

        return services;
    }

    public static IServiceCollection AddExternalServiceOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExternalConnectionInfo>(configuration.GetSection(nameof(ExternalConnectionInfo)));

        return services;
    }
}