using Task3.Dal.Models.Enums;
using Task3.Dal.Serializators;

namespace GrpcServer.Extensions;

public static class OrderHistoryDataExtensions
{
    public static OrderHistoryPayload? ToOrderHistoryData(
        this OrderHistoryData? payloadDto)
    {
        if (payloadDto == null) return null;

        return payloadDto.Kind switch
        {
            OrderHistoryItemKind.Created => new OrderHistoryPayload
            {
                CreatedData = new CreatedData { Message = payloadDto.Message },
            },
            OrderHistoryItemKind.ItemAdded => new OrderHistoryPayload
            {
                ItemAddedData = new ItemAddedData
                {
                    ProductId = payloadDto.OrderId,
                    Quantity = int.Parse(payloadDto.Message.Split(" ").Last()),
                },
            },
            OrderHistoryItemKind.ItemRemoved => new OrderHistoryPayload
            {
                ItemRemovedData = new ItemRemovedData
                {
                    ProductId = payloadDto.OrderId,
                },
            },
            OrderHistoryItemKind.StateChanged => new OrderHistoryPayload
            {
                StateChangedData = new StateChangedData
                {
                    NewState = payloadDto.Message.Split(' ').Last(),
                },
            },
            _ => null,
        };
    }
}