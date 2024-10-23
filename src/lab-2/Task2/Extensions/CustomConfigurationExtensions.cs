using Microsoft.Extensions.Configuration;
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
        ConfigurationManager configurationManager,
        TimeSpan updateInterval)
    {
        services.AddRefitConfigurationClient().AddApiApplicationModelMappers();

        services.AddSingleton<CustomConfigurationProvider>();
        services.AddSingleton<CustomConfigurationProviderSource>();

        services.AddScoped(serviceProvider =>
        {
            CustomConfigurationProvider configurationProvider
                = serviceProvider.GetRequiredService<CustomConfigurationProvider>();
            IConfigurationClient client = serviceProvider.GetRequiredService<IConfigurationClient>();

            var configurationBuilder = (IConfigurationBuilder)configurationManager;
            configurationBuilder.Add(serviceProvider.GetRequiredService<CustomConfigurationProviderSource>());

            return new ConfigurationUpdateService(configurationProvider, client, updateInterval);
        });

        return services;
    }
}