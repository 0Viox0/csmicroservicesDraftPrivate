using GrpcClientHttpGateway.Models;
using GrpcClientHttpGateway.Models.PayloadModels;
using GrpcServer;

namespace GrpcClientHttpGateway.Extensions;

public static class OrderHistoryDataExtension
{
    public static OrderHistoryPayloadBase ToOrderHistoryPayload(this OrderHistoryPayload orderHistoryData)
    {
        OrderHistoryPayloadBase result = orderHistoryData.DataCase switch
        {
            OrderHistoryPayload.DataOneofCase.CreatedData => ToCreatedDataPayload(orderHistoryData.CreatedData),
            OrderHistoryPayload.DataOneofCase.ItemAddedData => ToItemAddedPayload(orderHistoryData.ItemAddedData),
            OrderHistoryPayload.DataOneofCase.ItemRemovedData => ToItemRemovedPayload(orderHistoryData.ItemRemovedData),
            OrderHistoryPayload.DataOneofCase.StateChangedData => ToStateChangedPayload(orderHistoryData.StateChangedData),
            OrderHistoryPayload.DataOneofCase.None or
            _ => throw new ArgumentOutOfRangeException(nameof(orderHistoryData), orderHistoryData, null),
        };

        return result;
    }

    public static OrderHistoryPayloadBase ToCreatedDataPayload(this CreatedData createdData)
    {
        return new CreatedByPayload(createdData.Message);
    }

    public static OrderHistoryPayloadBase ToItemAddedPayload(this ItemAddedData orderHistoryData)
    {
        return new ItemAddedPayload(
            orderHistoryData.ProductId,
            orderHistoryData.Quantity);
    }

    public static OrderHistoryPayloadBase ToItemRemovedPayload(this ItemRemovedData itemRemovedData)
    {
        return new ItemRemovedPayload(itemRemovedData.ProductId);
    }

    public static OrderHistoryPayloadBase ToStateChangedPayload(this StateChangedData stateChangedData)
    {
        return new StateChangedPayload(stateChangedData.NewState);
    }
}