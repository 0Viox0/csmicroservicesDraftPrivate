namespace Kafka.Options;

public class ConsumerOptions
{
    public string? Host { get; set; }

    public string? Port { get; set; }

    public string? Topic { get; set; }

    public int BatchSize { get; set; }

    public TimeSpan BatchTimeout { get; set; }

    public string ConnectionUrl => $"${Host}:{Port}";
}