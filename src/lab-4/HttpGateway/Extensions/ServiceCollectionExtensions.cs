using GrpcClientHttpGateway.Manager;
using GrpcClientHttpGateway.Mappers;

namespace GrpcClientHttpGateway.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcModelMapper(this IServiceCollection services)
    {
        services.AddScoped<GrpcModelMapper>();
        services.AddScoped<PayloadManager>();

        return services;
    }
}