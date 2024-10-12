using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task3.Bll.Mappers;
using Task3.Bll.Services;

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

    public static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        return services;
    }
}