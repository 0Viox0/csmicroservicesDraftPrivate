using Google.Protobuf;
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

        return services;
    }

    public static IServiceCollection AddProducer<TKey, TValue>(this IServiceCollection services)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.AddScoped<IMessageProducer<TKey, TValue>, MessageProducer<TKey, TValue>>();

        return services;
    }
}