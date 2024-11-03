using GrpcClientHttpGateway.Models;
using GrpcServer;

namespace GrpcClientHttpGateway.Mappers;

public class GrpcModelMapper
{
    public CreateOrderRequest ToCreateOrderRequest(CreatedByModel request)
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
            Price = (long)productCreationModel.Price,
        };
    }
}