namespace GrpcClientHttpGateway.Models;

public class OrderHistoryItemReturnModel
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string? CreatedAt { get; set; }

    public OrderHistoryItemKind OrderHistoryItemKind { get; set; }

    public object? Payload { get; set; }
}