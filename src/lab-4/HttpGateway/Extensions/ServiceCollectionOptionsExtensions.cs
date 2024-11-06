using GrpcClientHttpGateway.GrpcConfigurationModels;

namespace GrpcClientHttpGateway.Extensions;

public static class ServiceCollectionOptionsExtensions
{
    public static IServiceCollection AddOrdersServiceOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GrpcClientOptions>()
            .Bind(configuration.GetSection(nameof(GrpcClientOptions)));

        return services;
    }
}