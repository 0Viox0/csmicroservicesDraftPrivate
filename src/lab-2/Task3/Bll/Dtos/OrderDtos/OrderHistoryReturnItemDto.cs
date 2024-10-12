using Task3.Dal.Models.Enums;

namespace Task3.Bll.Dtos.OrderDtos;

public class OrderHistoryReturnItemDto
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public OrderHistoryItemKind Kind { get; set; }

    public string? Payload { get; set; }
}