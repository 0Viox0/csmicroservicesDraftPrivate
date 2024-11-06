using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Services;
using Enum = System.Enum;

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

        await foreach (OrderHistoryReturnItemDto itemDto in history)
        {
            var orderHistoryItem = new OrderHistoryItem
            {
                Id = itemDto.Id,
                OrderId = itemDto.OrderId,
                CreatedAt = itemDto.CreatedAt.ToString("o"),
                Kind = (OrderHistoryItemKind)Enum.Parse(
                    typeof(OrderHistoryItemKind),
                    itemDto.Kind.ToString(),
                    ignoreCase: true),
                Payload = MapPayload(itemDto.Payload),
            };

            response.OrderHistory.Add(orderHistoryItem);
        }

        return response;
    }

    // helper method for mapping application payloadDto to the grpc payload
    private OrderHistoryData? MapPayload(Task3.Dal.Serializators.OrderHistoryData? payloadDto)
    {
        if (payloadDto == null) return null;

        return payloadDto.Kind switch
        {
            Task3.Dal.Models.Enums.OrderHistoryItemKind.Created => new OrderHistoryData
            {
                CreatedData = new CreatedData { Message = payloadDto.Message },
            },
            Task3.Dal.Models.Enums.OrderHistoryItemKind.ItemAdded => new OrderHistoryData
            {
                ItemAddedData = new ItemAddedData
                {
                    ProductId = payloadDto.OrderId,
                    Quantity = int.Parse(payloadDto.Message.Split(" ").Last()),
                },
            },
            Task3.Dal.Models.Enums.OrderHistoryItemKind.ItemRemoved => new OrderHistoryData
            {
                ItemRemovedData = new ItemRemovedData
                {
                    ProductId = payloadDto.OrderId,
                },
            },
            Task3.Dal.Models.Enums.OrderHistoryItemKind.StateChanged => new OrderHistoryData
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