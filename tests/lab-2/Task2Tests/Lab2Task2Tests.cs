using FluentAssertions;
using Microsoft.Extensions.Primitives;
using Task1.Models.ApplicationConfigurationModels;
using Task2.ConfigurationProviders;

namespace Task2Tests;

public class Lab2Task2Tests
{
    private readonly CustomConfigurationProvider _provider;

    public Lab2Task2Tests()
    {
        _provider = new CustomConfigurationProvider();
    }

    [Fact]
    public void Scenario1_ShouldAddSingleKeyValuePairAndUpdateConfiguration()
    {
        var items = new List<ConfigurationKeyValueItem>
        {
            new("key1", "value1"),
        };

        IChangeToken reloadTokenBeforeUpdate = _provider.GetReloadToken();
        _provider.UpdateConfiguration(items);

        _provider.TryGet("DatabaseSettings:key1", out string? value).Should().BeTrue();
        value.Should().Be("value1");

        _provider.GetReloadToken().Should().NotBe(reloadTokenBeforeUpdate);
    }

    [Fact]
    public void Scenario2_ShouldNotCallUpdateWhenSameConfigurationPassed()
    {
        var items = new List<ConfigurationKeyValueItem>
        {
            new("key1", "value1"),
        };

        _provider.UpdateConfiguration(items);

        IChangeToken reloadTokenBeforeUpdate = _provider.GetReloadToken();
        _provider.UpdateConfiguration(items);

        _provider.TryGet("DatabaseSettings:key1", out string? value).Should().BeTrue();
        value.Should().Be("value1");

        _provider.GetReloadToken().Should().Be(reloadTokenBeforeUpdate);
    }

    [Fact]
    public void Scenario3_ShouldUpdateKeyValueAndReloadConfiguration()
    {
        var initialItems = new List<ConfigurationKeyValueItem>
        {
            new("key1", "value1"),
        };

        _provider.UpdateConfiguration(initialItems);

        var updatedItems = new List<ConfigurationKeyValueItem>
        {
            new("key1", "value2"),
        };

        IChangeToken reloadTokenBeforeUpdate = _provider.GetReloadToken();
        _provider.UpdateConfiguration(updatedItems);

        _provider.TryGet("DatabaseSettings:key1", out string? value).Should().BeTrue();
        value.Should().Be("value2");

        _provider.GetReloadToken().Should().NotBe(reloadTokenBeforeUpdate);
    }

    [Fact]
    public void Scenario4_ShouldClearConfigurationWhenPassedEmptyList()
    {
        var initialItems = new List<ConfigurationKeyValueItem>
        {
            new("key1", "value1"),
        };

        _provider.UpdateConfiguration(initialItems);

        IChangeToken reloadTokenBeforeUpdate = _provider.GetReloadToken();
        _provider.UpdateConfiguration([]);

        _provider.TryGet("DatabaseSettings:key1", out _).Should().BeFalse();
        _provider.GetReloadToken().Should().NotBe(reloadTokenBeforeUpdate);
    }
}