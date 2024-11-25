using Confluent.Kafka;
using Google.Protobuf;

namespace Kafka.Serializers;

public class ProtobufSerializer<T> : ISerializer<T> where T : IMessage<T>, new()
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return data.ToByteArray();
    }
}