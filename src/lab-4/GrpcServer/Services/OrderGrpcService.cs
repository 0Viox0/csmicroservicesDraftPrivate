using Grpc.Core;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Services;

namespace GrpcServer.Services;

public class OrderGrpcService : OrdersService.OrdersServiceBase
{
    private readonly OrderService _orderService;

    public OrderGrpcService(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task<CreateOrderResponse> CreateOrder(
        CreateOrderRequest request,
        ServerCallContext context)
    {
        long orderId = await _orderService.CreateOrder(request.CreatedBy, context.CancellationToken);

        return new CreateOrderResponse
        {
            OrderId = orderId,
        };
    }

    public override async Task<AddProductToOrderResponse> AddProductToOrder(
        AddProductToOrderRequest request,
        ServerCallContext context)
    {
        var orderItemCreationDto = new OrderItemCreationDto
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
        };

        await _orderService.AddProductToOrder(orderItemCreationDto, context.CancellationToken);

        return new AddProductToOrderResponse();
    }
}