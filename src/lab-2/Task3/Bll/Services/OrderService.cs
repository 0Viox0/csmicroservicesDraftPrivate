using System.Text.Json;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Mappers;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;
using Task3.Dal.Repositories;

namespace Task3.Bll.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly OrderHistoryRepository _orderHistoryRepository;
    private readonly OrderHistoryMapper _orderHistoryMapper;

    public OrderService(
        OrderRepository orderRepository,
        OrderItemRepository orderItemRepository,
        OrderHistoryRepository orderHistoryRepository,
        OrderHistoryMapper orderHistoryMapper)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _orderHistoryMapper = orderHistoryMapper;
    }

    public async Task<long> CreateOrder(string createdBy, CancellationToken cancellationToken)
    {
        long orderId = await _orderRepository.CreateOrder(createdBy, cancellationToken).ConfigureAwait(false);

        await LogOrderHistory(
            orderId,
            OrderHistoryItemKind.Created,
            $"Order created by {createdBy}",
            cancellationToken).ConfigureAwait(false);
        return orderId;
    }

    public async Task AddProductToOrder(OrderItemCreationDto orderItemCreationDto, CancellationToken cancellationToken)
    {
        Order order =
            (await _orderRepository
                .SearchOrders(0, 1, cancellationToken, [orderItemCreationDto.OrderId])
                .ConfigureAwait(false))
            .First();

        if (order.State != OrderState.Created)
            return;

        await _orderItemRepository
            .CreateOrderItem(orderItemCreationDto, cancellationToken)
            .ConfigureAwait(false);

        await LogOrderHistory(
                orderItemCreationDto.OrderId,
                OrderHistoryItemKind.ItemAdded,
                $"Product {orderItemCreationDto.ProductId} added to order {orderItemCreationDto.OrderId}. Quantity: {orderItemCreationDto.Quantity}",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RemoveProductFromOrder(OrderItemRemoveDto orderItemProductRemoveDto, CancellationToken cancellationToken)
    {
        Order order
            = (await _orderRepository.SearchOrders(
                    0,
                    1,
                    cancellationToken,
                    [orderItemProductRemoveDto.OrderId])
                .ConfigureAwait(false))
            .First();

        if (order is not { State: OrderState.Created })
            return;

        OrderItem orderItem =
            (await _orderItemRepository
                .SearchOrderItems(
                    0,
                    1,
                    cancellationToken,
                    orderId: orderItemProductRemoveDto.OrderId,
                    productId: orderItemProductRemoveDto.ProductId)
                .ConfigureAwait(false)).First();

        await _orderItemRepository.SoftDeleteItem(
                orderItem.Id,
                cancellationToken)
            .ConfigureAwait(false);

        await LogOrderHistory(
                orderItem.OrderId,
                OrderHistoryItemKind.ItemRemoved,
                $"Product removed from order {orderItem.OrderId}. Item ID: {orderItemProductRemoveDto.ProductId}",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task TransferOrderToProcessing(long orderId, CancellationToken cancellationToken)
    {
        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Processing, cancellationToken)
            .ConfigureAwait(false);

        await LogOrderHistory(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order transferred to processing.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task FulfillOrder(long orderId, CancellationToken cancellationToken)
    {
        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Completed, cancellationToken)
            .ConfigureAwait(false);

        await LogOrderHistory(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order fulfilled.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CancelOrder(long orderId, CancellationToken cancellationToken)
    {
        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Cancelled, cancellationToken)
            .ConfigureAwait(false);

        await LogOrderHistory(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order cancelled.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrderHistoryReturnItemDto>> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        CancellationToken cancellationToken)
    {
        return (await _orderHistoryRepository
                .GetOrderHistory(orderHistoryItemSearchDto, cancellationToken)
                .ConfigureAwait(false))
            .Select(_orderHistoryMapper.ToOrderHistoryReturnItemDto);
    }

    private async Task LogOrderHistory(
        long orderId,
        OrderHistoryItemKind kind,
        string payload,
        CancellationToken cancellationToken)
    {
        var orderHistoryData = new
        {
            OrderId = orderId,
            State = kind.ToString(),
            Description = payload,
        };

        string jsonFormattedPayload = JsonSerializer.Serialize(orderHistoryData);

        await _orderHistoryRepository
            .CreateOrderHistory(orderId, kind, jsonFormattedPayload, cancellationToken)
            .ConfigureAwait(false);
    }
}