using Task3.Bll.Dtos.OrderDtos;
using Task3.Dal.Models;

namespace Task3.Bll.Mappers;

public class OrderHistoryMapper
{
    public OrderHistoryReturnItemDto ToOrderHistoryReturnItemDto(OrderHistoryItem orderHistoryItem)
    {
        return new OrderHistoryReturnItemDto()
        {
            Id = orderHistoryItem.Id,
            CreatedAt = orderHistoryItem.CreatedAt,
            Kind = orderHistoryItem.Kind,
            OrderId = orderHistoryItem.OrderId,
            Payload = orderHistoryItem.Payload,
        };
    }
}