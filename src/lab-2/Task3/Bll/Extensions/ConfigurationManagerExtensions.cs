using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Task3.Bll.Extensions;

public static class ConfigurationManagerExtensions
{
    public static IServiceCollection AddConfigurationManagerBasePath(
        this IServiceCollection services,
        ConfigurationManager configurationManger)
    {
        string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Task3"));

        configurationManger
            .SetBasePath(projectDirectory)
            .AddJsonFile("externalServiceConnectionInfo.json");

        return services;
    }
}