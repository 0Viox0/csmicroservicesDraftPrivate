using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task1.Interfaces;
using Task2.ConfigurationProviders;
using Task2.Service;

namespace Task2.Extensions;

public static class CustomConfigurationExtensions
{
    public static IServiceCollection AddCustomConfiguration(
        this IServiceCollection services,
        TimeSpan updateInterval)
    {
        services.AddRefitConfigurationClient();
        services.AddSingleton<CustomConfigurationProvider>();
        services.AddSingleton<CustomConfigurationProviderSource>();

        services.AddSingleton(serviceProvider =>
        {
            CustomConfigurationProvider configurationProvider
                = serviceProvider.GetRequiredService<CustomConfigurationProvider>();

            IConfigurationClient client = serviceProvider.GetRequiredService<IConfigurationClient>();

            return new ConfigurationUpdateService(configurationProvider, client, updateInterval);
        });

        return services;
    }
}