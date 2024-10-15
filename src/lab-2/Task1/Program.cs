using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task1.Interfaces;
using Task1.Models.ApplicationConfigurationModels;

internal class Program
{
    public static async Task Main(string[] args)
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddHttpClientConfigurationClient("http://localhost:8080")

            // .AddRefitConfigurationClient("http://localhost:8080")
            .BuildServiceProvider();

        IConfigurationClient configurationClient
            = serviceProvider.GetRequiredService<IConfigurationClient>();

        ConfigurationKeyValueCollectionWIthPageToken response = await
            configurationClient
                .GetConfigurationsAsync(10, null, CancellationToken.None)
                .ConfigureAwait(false);

        foreach (ConfigurationKeyValueItem configurationItemDto in response.ConfigurationKeyValueItems)
        {
            Console.Out.WriteLine(configurationItemDto);
        }
    }
}