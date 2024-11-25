using Bll.Dtos.OrderDtos;
using Dal.Models;

namespace Bll.Mappers;

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