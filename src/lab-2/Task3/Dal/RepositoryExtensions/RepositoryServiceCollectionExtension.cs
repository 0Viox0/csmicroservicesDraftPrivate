using Microsoft.Extensions.DependencyInjection;
using Task3.Dal.Repositories;

namespace Task3.Dal.RepositoryExtensions;

public static class RepositoryServiceCollectionExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ProductRepository>();
        services.AddSingleton<OrderRepository>();
        services.AddSingleton<OrderItemRepository>();

        return services;
    }
}