using Microsoft.Extensions.Configuration;

namespace Task2.ConfigurationProviders;

public class CustomConfigurationProviderSource : IConfigurationSource
{
    private readonly CustomConfigurationProvider _provider;

    public CustomConfigurationProviderSource(CustomConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => _provider;
}