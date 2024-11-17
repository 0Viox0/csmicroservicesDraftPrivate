using GrpcServer.Mappers;

namespace GrpcServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlMapper(this IServiceCollection services)
    {
        services.AddScoped<ProductMapper>();

        return services;
    }
}