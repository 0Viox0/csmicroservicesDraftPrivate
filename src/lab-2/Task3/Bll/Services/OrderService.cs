using System.Transactions;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Mappers;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;
using Task3.Dal.Repositories;
using Task3.Dal.Serializators;

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
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        long orderId = await _orderRepository
            .CreateOrder(createdBy, cancellationToken);

        var orderHistoryData =
            new OrderHistoryData(
                orderId,
                OrderHistoryItemKind.Created,
                $"Order created by {createdBy}");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();

        return orderId;
    }

    public async Task AddProductToOrder(OrderItemCreationDto orderItemCreationDto, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order =
            await _orderRepository
                .SearchOrders(0, 1,  cancellationToken, [orderItemCreationDto.OrderId])
                .FirstAsync(cancellationToken);

        if (order.State != OrderState.Created)
            return;

        await _orderItemRepository
            .CreateOrderItem(orderItemCreationDto, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderItemCreationDto.OrderId,
            OrderHistoryItemKind.ItemAdded,
            $"Product {orderItemCreationDto.ProductId} added to order {orderItemCreationDto.OrderId}. Quantity: {orderItemCreationDto.Quantity}");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public async Task RemoveProductFromOrder(OrderItemRemoveDto orderItemProductRemoveDto, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order order =
            await _orderRepository
                .SearchOrders(0, 1, cancellationToken, [orderItemProductRemoveDto.OrderId])
                .FirstAsync(cancellationToken);

        if (order is not { State: OrderState.Created })
            return;

        OrderItem orderItem =
            await _orderItemRepository
                .SearchOrderItems(0, 1, cancellationToken, orderId: orderItemProductRemoveDto.OrderId, productId: orderItemProductRemoveDto.ProductId)
                .FirstAsync(cancellationToken: cancellationToken);

        await _orderItemRepository
            .SoftDeleteItem(orderItem.Id, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderItem.OrderId,
            OrderHistoryItemKind.ItemRemoved,
            $"Product removed from order {orderItem.OrderId}. Item ID: {orderItemProductRemoveDto.ProductId}");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public async Task TransferOrderToProcessing(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Processing, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderId,
            OrderHistoryItemKind.StateChanged,
            "Order transferred to processing.");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public async Task FulfillOrder(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Completed, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderId,
            OrderHistoryItemKind.StateChanged,
            "Order fulfilled.");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public async Task CancelOrder(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Cancelled, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderId,
            OrderHistoryItemKind.StateChanged,
            "Order cancelled.");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public IAsyncEnumerable<OrderHistoryReturnItemDto> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        IAsyncEnumerable<OrderHistoryReturnItemDto> orderHistoryReturnItems =
            _orderHistoryRepository
                .GetOrderHistory(orderHistoryItemSearchDto, cancellationToken)
                .Select(_orderHistoryMapper.ToOrderHistoryReturnItemDto);

        scope.Complete();

        return orderHistoryReturnItems;
    }

    private async Task LogOrderHistory(
        OrderHistoryData orderHistoryData,
        CancellationToken cancellationToken)
    {
        await _orderHistoryRepository
            .CreateOrderHistory(orderHistoryData, cancellationToken);
    }
}