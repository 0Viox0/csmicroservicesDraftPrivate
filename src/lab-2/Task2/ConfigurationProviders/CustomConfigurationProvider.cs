using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.ConfigurationProviders;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public void UpdateConfiguration(IEnumerable<ConfigurationItemDto> items)
    {
        var itemList = items.ToList();
        bool configurationChanged = false;

        if (itemList.Count == 0)
        {
            if (Data.Count > 0)
            {
                Data.Clear();
                configurationChanged = true;
            }
        }
        else
        {
            foreach (ConfigurationItemDto keyValuePair in itemList)
            {
                if (!Data.TryGetValue($"DatabaseSettings:{keyValuePair.Key}", out string? existingValue)
                    || existingValue != keyValuePair.Value)
                {
                    Data[$"DatabaseSettings:{keyValuePair.Key}"] = keyValuePair.Value;
                    configurationChanged = true;
                }
            }

            var keysToRemove = Data.Keys.Except(itemList.Select(item => $"DatabaseSettings:{item.Key}")).ToList();
            foreach (string? key in keysToRemove)
            {
                Data.Remove($"DatabaseSettings:{key}");
                configurationChanged = true;
            }
        }

        if (configurationChanged)
        {
            OnReload();
        }
    }
}