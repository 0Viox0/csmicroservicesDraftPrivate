using Google.Protobuf;
using Kafka.Consumer;
using Kafka.Consumer.Services;
using Kafka.Producer;
using Kafka.Producer.Services;
using Kafka.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddProtoSerializer(this IServiceCollection services)
    {
        services.AddScoped(typeof(ProtobufSerializer<>));
        services.AddScoped(typeof(ProtobufDeserializer<>));

        return services;
    }

    public static IServiceCollection AddProducer<TKey, TValue>(this IServiceCollection services)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.AddScoped<IMessageProducer<TKey, TValue>, MessageProducer<TKey, TValue>>();

        return services;
    }

    public static IServiceCollection AddConsumer<TKey, TValue>(this IServiceCollection services)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.AddScoped<
            IMessageConsumerChannelWriter<TKey, TValue>,
            ConsumedMessageChannelWriter<TKey, TValue>>();

        services.AddScoped<
            IMessageConsumerChannelReader<TKey, TValue>,
            ConsumedMessageChannelReader<TKey, TValue>>();

        services.AddHostedService<BatchConsumerBackgroundService<TKey, TValue>>();

        return services;
    }
}