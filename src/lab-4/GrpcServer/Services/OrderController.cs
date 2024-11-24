using Bll.Dtos.OrderDtos;
using Bll.Services;
using Dal.Models;
using Dal.Repositories;
using Grpc.Core;
using GrpcServer.Extensions;

namespace GrpcServer.Services;

public class OrderController : OrdersService.OrdersServiceBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
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

    public override async Task<RemoveProductFromOrderResponse> RemoveProductFromOrder(
        RemoveProductFromOrderRequest request,
        ServerCallContext context)
    {
        var orderItemRemoveDto = new OrderItemRemoveDto
        {
            OrderId = request.OrderId,
            ProductId = request.ProductId,
        };

        await _orderService.RemoveProductFromOrder(orderItemRemoveDto, context.CancellationToken);

        return new RemoveProductFromOrderResponse();
    }

    public override async Task<TransferOrderToProcessingResponse> TransferOrderToProcessing(
        TransferOrderToProcessingRequest request,
        ServerCallContext context)
    {
        await _orderService.TransferOrderToProcessing(request.OrderId, context.CancellationToken);

        return new TransferOrderToProcessingResponse();
    }

    public override async Task<FulfillOrderResponse> FulfillOrder(
        FulfillOrderRequest request,
        ServerCallContext context)
    {
        await _orderService.FulfillOrder(request.OrderId, context.CancellationToken);

        return new FulfillOrderResponse();
    }

    public override async Task<CancelOrderResponse> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        await _orderService.CancelOrder(request.OrderId, context.CancellationToken);

        return new CancelOrderResponse();
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

        await foreach (OrderHistoryReturnItemDto itemDto in history)
        {
            var orderHistoryItem = new OrderHistoryItem
            {
                Id = itemDto.Id,
                OrderId = itemDto.OrderId,
                CreatedAt = itemDto.CreatedAt.ToString("o"),
                Kind = (OrderEventType)itemDto.Kind,
                Payload = itemDto.Payload.ToOrderHistoryData(),
            };

            response.OrderHistory.Add(orderHistoryItem);
        }

        return response;
    }
}