using Google.Protobuf.WellKnownTypes;
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

    public override async Task<Empty> AddProductToOrder(
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

        return new Empty();
    }

    public override async Task<Empty> RemoveProductFromOrder(
        RemoveProductFromOrderRequest request,
        ServerCallContext context)
    {
        var orderItemRemoveDto = new OrderItemRemoveDto
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
        };

        await _orderService.RemoveProductFromOrder(orderItemRemoveDto, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> TransferOrderToProcessing(
        TransferOrderToProcessingRequest request,
        ServerCallContext context)
    {
        await _orderService.TransferOrderToProcessing(request.OrderId, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> FulfillOrder(
        FulfillOrderRequest request,
        ServerCallContext context)
    {
        await _orderService.FulfillOrder(request.OrderId, context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        await _orderService.CancelOrder(request.OrderId, context.CancellationToken);

        return new Empty();
    }

    public override async Task<GetOrderHistoryResponse> GetOrderHistory(
        GetOrderHistoryRequest request,
        ServerCallContext context)
    {
        var searchDto = new OrderHistoryItemSearchDto
        {
            OrderId = request.OrderId,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize,
        };

        IAsyncEnumerable<OrderHistoryReturnItemDto> history =
            await _orderService.GetOrderHistory(searchDto, context.CancellationToken);

        var response = new GetOrderHistoryResponse();

        // Convert each OrderHistoryReturnItemDto to an OrderHistoryItem
        await foreach (OrderHistoryReturnItemDto itemDto in history)
        {
            var orderHistoryItem = new OrderHistoryItem
            {
                Id = itemDto.Id,
                OrderId = itemDto.OrderId,
                CreatedAt = itemDto.CreatedAt.ToString("o"),
                Kind = (OrderHistoryItemKind)itemDto.Kind,
                Payload = itemDto.Payload != null ? new OrderHistoryData
                {
                    OrderId = itemDto.Payload.OrderId,
                    Kind = (OrderHistoryItemKind)itemDto.Payload.Kind,
                    Message = itemDto.Payload.Message,
                }
                : null,
            };

            response.OrderHistory.Add(orderHistoryItem);
        }

        return response;
    }
}