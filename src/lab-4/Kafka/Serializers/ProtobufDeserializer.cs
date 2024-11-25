using Confluent.Kafka;
using Google.Protobuf;

namespace Kafka.Serializers;

public class ProtobufDeserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new(() => new T());

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return isNull ? throw new ArgumentException("the data is null") : Parser.ParseFrom(data);
    }
}