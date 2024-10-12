using Microsoft.Extensions.DependencyInjection;
using Task3.Dal.Repositories;

namespace Task3.Dal.RepositoryExtensions;

public static class RepositoryServiceCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ProductRepository>();
        services.AddScoped<OrderRepository>();
        services.AddScoped<OrderItemRepository>();
        services.AddScoped<OrderHistoryRepository>();

        return services;
    }
}