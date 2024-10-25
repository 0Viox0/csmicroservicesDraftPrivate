using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Task1.ConfigurationModels;
using Task1.Implementations;
using Task1.Interfaces;
using Task1.Mappers;

namespace Task1.Extensions;

public static class ConfigurationClientExtensions
{
    public static IServiceCollection AddHttpClientConfigurationClient(
        this IServiceCollection services,
        string baseUrl)
    {
        services.AddHttpClient(
            "ConfigurationServiceClient",
            client => client.BaseAddress = new Uri(baseUrl));

        services.AddTransient<IConfigurationClient, HttpClientConfigurationClient>();

        return services;
    }

    public static IServiceCollection AddRefitConfigurationClient(
        this IServiceCollection services)
    {
        services.AddRefitClient<IRefitConfigurationClient>()
            .ConfigureHttpClient((serviceProvider, c) =>
            {
                ExternalConnectionInfo externalConnectionInfo =
                    serviceProvider.GetRequiredService<IOptions<ExternalConnectionInfo>>().Value;

                string baseUrl = $"http://{externalConnectionInfo.Host}:{externalConnectionInfo.Port}";
                c.BaseAddress = new Uri(baseUrl);
            });

        services.AddTransient<IConfigurationClient, RefitConfigurationClient>();

        return services;
    }

    public static IServiceCollection AddApiApplicationModelMappers(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ApiApplicationModelConfigurationMapper>();

        return serviceCollection;
    }
}