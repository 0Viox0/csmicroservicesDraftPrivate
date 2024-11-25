using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bll.Extensions;

public static class ConfigurationManagerExtensions
{
    public static IServiceCollection AddConfigurationManagerBasePath(
        this IServiceCollection services,
        ConfigurationManager configurationManger)
    {
        string jsonDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Task3MainFunction"));

        configurationManger
            .SetBasePath(jsonDirectory)
            .AddJsonFile("externalServiceConnectionInfo.json");

        return services;
    }
}