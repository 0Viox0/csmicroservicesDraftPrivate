using Task3.Dal.Models.Enums;

namespace Task3.Dal.Models;

public class OrderHistoryItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public OrderHistoryItemKind Kind { get; set; }

    public string? Payload { get; set; }
}