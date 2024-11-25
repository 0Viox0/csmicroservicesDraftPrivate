using Kafka.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddKafkaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ProducerOptions>()
            .Bind(configuration.GetSection(nameof(ProducerOptions)));

        services.AddOptions<ConsumerOptions>()
            .Bind(configuration.GetSection(nameof(ConsumerOptions)));

        return services;
    }
}