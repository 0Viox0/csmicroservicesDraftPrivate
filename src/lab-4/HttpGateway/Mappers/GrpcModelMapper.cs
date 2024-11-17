using Google.Type;
using GrpcClientHttpGateway.Extensions;
using GrpcClientHttpGateway.Models;
using GrpcClientHttpGateway.Models.PayloadModels;
using GrpcServer;
using OrderHistoryItemKind = GrpcClientHttpGateway.Models.OrderHistoryItemKind;

namespace GrpcClientHttpGateway.Mappers;

public class GrpcModelMapper
{
    public CreateOrderRequest ToCreateOrderRequest(CreatedByPayload request)
    {
        return new CreateOrderRequest
        {
            CreatedBy = request.CreatedBy,
        };
    }

    public AddProductToOrderRequest ToAddProductToOrderRequest(
        long orderId,
        long productId,
        int quantity)
    {
        return new AddProductToOrderRequest
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };
    }

    public RemoveProductFromOrderRequest ToRemoveProductFromOrderRequest(
        long orderId,
        long productId)
    {
        return new RemoveProductFromOrderRequest
        {
            OrderId = orderId,
            ProductId = productId,
        };
    }

    public GetOrderHistoryRequest ToGetOrderHistoryRequest(
        long orderId,
        int pageIndex,
        int pageSize)
    {
        return new GetOrderHistoryRequest
        {
            OrderId = orderId,
            PageIndex = pageIndex,
            PageSize = pageSize,
        };
    }

    public CreateProductRequest ToCreateProductRequest(ProductCreationModel productCreationModel)
    {
        return new CreateProductRequest
        {
            Name = productCreationModel.Name,
            Price = new Money { DecimalValue = productCreationModel.Price },
        };
    }

    public OrderHistoryItemReturnModel ToOrderHistoryItemReturnModel(OrderHistoryItem orderHistoryItem)
    {
        var itemReturnModel = new OrderHistoryItemReturnModel(
            orderHistoryItem.Id,
            orderHistoryItem.OrderId,
            orderHistoryItem.CreatedAt,
            (OrderHistoryItemKind)orderHistoryItem.Kind,
            orderHistoryItem.Payload.ToOrderHistoryPayload());

        return itemReturnModel;
    }
}