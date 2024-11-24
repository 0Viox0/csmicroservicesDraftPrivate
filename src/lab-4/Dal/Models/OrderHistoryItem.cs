using Dal.Models.Enums;
using Dal.Serializators;

namespace Dal.Models;

public class OrderHistoryItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public OrderHistoryItemKind Kind { get; set; }

    public OrderHistoryData? Payload { get; set; }
}