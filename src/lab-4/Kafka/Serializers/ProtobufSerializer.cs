using Confluent.Kafka;
using Google.Protobuf;

namespace Kafka.Serializers;

public class ProtobufSerializer<T> : ISerializer<T>, IDeserializer<T> where T : IMessage<T>, new()
{
    private static readonly MessageParser<T> Parser = new(() => new T());

    public byte[] Serialize(T data, SerializationContext context)
    {
        return data.ToByteArray();
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return isNull ? throw new ArgumentException("the data is null") : Parser.ParseFrom(data);
    }
}