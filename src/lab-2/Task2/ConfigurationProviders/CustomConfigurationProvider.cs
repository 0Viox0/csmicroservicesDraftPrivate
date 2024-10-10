using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.ConfigurationProviders;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public void UpdateConfiguration(IEnumerable<ConfigurationItemDto> items)
    {
        foreach (ConfigurationItemDto keyValuePair in items)
        {
            Data[keyValuePair.Key] = keyValuePair.Value;
        }

        OnReload();
    }
}