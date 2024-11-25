using GrpcClientHttpGateway.GrpcConfigurationModels;
using GrpcServer;
using Microsoft.Extensions.Options;
using Orders.ProcessingService.Contracts;

namespace GrpcClientHttpGateway.Extensions;

public static class ServiceCollectionGrpcServicesExtensions
{
    public static IServiceCollection AddHttpOrdersGrpcClient(this IServiceCollection services)
    {
        services.AddGrpcClient<OrdersService.OrdersServiceClient>((serviceProvider, o) =>
        {
            GrpcClientOptions options = serviceProvider.GetRequiredService<IOptions<GrpcClientOptions>>().Value;

            o.Address = new Uri(options.OrdersServiceUrl);
        });

        return services;
    }

    public static IServiceCollection AddHttpProductsGrpcClient(this IServiceCollection services)
    {
        services.AddGrpcClient<ProductsService.ProductsServiceClient>((serviceProvider, o) =>
        {
            GrpcClientOptions options = serviceProvider.GetRequiredService<IOptions<GrpcClientOptions>>().Value;

            o.Address = new Uri(options.ProductsServiceUrl);
        });

        return services;
    }

    public static IServiceCollection AddHttpOrderProcessingGrpcClient(this IServiceCollection services)
    {
        services.AddGrpcClient<OrderService.OrderServiceClient>((serviceProvider, o) =>
        {
            GrpcClientOptions options = serviceProvider.GetRequiredService<IOptions<GrpcClientOptions>>().Value;

            o.Address = new Uri(options.OrderProcessingServiceUrl);
        });

        return services;
    }
}