namespace Kafka.Options;

public class ProducerOptions
{
    public string? Host { get; set; }

    public string? Port { get; set; }

    public string? Topic { get; set; }

    public string ConnectionUrl => $"{Host}:{Port}";
}