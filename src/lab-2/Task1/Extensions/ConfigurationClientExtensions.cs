using Microsoft.Extensions.DependencyInjection;
using Refit;
using Task1.Implementations;
using Task1.Interfaces;

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
        this IServiceCollection services,
        string baseUrl)
    {
        services.AddRefitClient<IRefitConfigurationClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));

        services.AddTransient<IConfigurationClient, RefitConfigurationClient>();

        return services;
    }
}