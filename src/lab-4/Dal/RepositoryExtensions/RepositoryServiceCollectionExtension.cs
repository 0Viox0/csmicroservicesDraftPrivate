using Dal.Repositories;
using Dal.Serializators;
using Microsoft.Extensions.DependencyInjection;

namespace Dal.RepositoryExtensions;

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

    public static IServiceCollection AddOrderHistoryDataJsonSerializer(this IServiceCollection services)
    {
        services.AddScoped<IOrderHistoryDataSerializer, OrderHistoryJsonDataSerializer>();

        return services;
    }
}