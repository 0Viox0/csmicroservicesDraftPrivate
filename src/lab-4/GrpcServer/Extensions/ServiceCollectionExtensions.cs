using GrpcServer.Mappers;

namespace GrpcServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductMapper(this IServiceCollection services)
    {
        services.AddScoped<ProductMapper>();

        return services;
    }
}