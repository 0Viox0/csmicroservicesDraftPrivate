using GrpcClientHttpGateway.Models;
using GrpcClientHttpGateway.Models.PayloadModels;
using GrpcServer;

namespace GrpcClientHttpGateway.Extensions;

public static class OrderHistoryDataExtension
{
    public static OrderHistoryPayloadBase ToOrderHistoryPayload(this OrderHistoryData orderHistoryData)
    {
        OrderHistoryPayloadBase result = orderHistoryData switch
        {
            { CreatedData: not null } => new CreatedByPayload(orderHistoryData.CreatedData.Message),
            { ItemAddedData: not null } => new ItemAddedPayload(
                orderHistoryData.ItemAddedData.ProductId,
                orderHistoryData.ItemAddedData.Quantity),
            { ItemRemovedData: not null } => new ItemRemovedPayload(orderHistoryData.ItemRemovedData.ProductId),
            { StateChangedData: not null } => new StateChangedPayload(orderHistoryData.StateChangedData.NewState),
            _ => throw new InvalidOperationException("No valid data found in OrderHistoryData"),
        };

        return result;
    }
}