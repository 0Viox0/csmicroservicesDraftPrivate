using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task1.Interfaces;
using Task1.Models;

ServiceProvider serviceProvider = new ServiceCollection()
    .AddHttpClientConfigurationClient("http://localhost:8080")

    // .AddRefitConfigurationClient("http://localhost:8080")
    .BuildServiceProvider();

IConfigurationClient configurationClient
    = serviceProvider.GetRequiredService<IConfigurationClient>();

QueryConfigurationsResponse response = await
    configurationClient
        .GetConfigurationsAsync(10, null, CancellationToken.None)
        .ConfigureAwait(false);

foreach (ConfigurationItemDto configurationItemDto in response.Items)
{
    Console.Out.WriteLine(configurationItemDto);
}