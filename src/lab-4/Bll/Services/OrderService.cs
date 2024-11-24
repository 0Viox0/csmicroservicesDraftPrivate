using Bll.CustomExceptions;
using Bll.Dtos.OrderDtos;
using Bll.Mappers;
using Dal.Models;
using Dal.Models.Enums;
using Dal.Repositories;
using Dal.Serializators;
using Kafka.Producer;
using Orders.Kafka.Contracts;
using System.Transactions;

namespace Bll.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly OrderHistoryRepository _orderHistoryRepository;
    private readonly OrderHistoryMapper _orderHistoryMapper;
    private readonly IMessageProducer<OrderCreationKey, OrderCreationValue> _messageProducer;

    public OrderService(
        OrderRepository orderRepository,
        OrderItemRepository orderItemRepository,
        OrderHistoryRepository orderHistoryRepository,
        OrderHistoryMapper orderHistoryMapper,
        IMessageProducer<OrderCreationKey, OrderCreationValue> messageProducer)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _orderHistoryMapper = orderHistoryMapper;
        _messageProducer = messageProducer;
    }

    public async Task<long> CreateOrder(string createdBy, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        long orderId = await _orderRepository
            .CreateOrder(createdBy, cancellationToken);

        if (orderId == -1)
        {
            throw new OrderException("order cound not be created");
        }

        var orderHistoryData =
            new OrderHistoryData(
                orderId,
                OrderHistoryItemKind.Created,
                $"Order created by {createdBy}");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        // TODO: add producing the creation message here

        scope.Complete();

        return orderId;
    }

    public async Task AddProductToOrder(OrderItemCreationDto orderItemCreationDto, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Order? order = await _orderRepository.GetOrderById(orderItemCreationDto.OrderId, cancellationToken);

        if (order == null)
        {
            throw new OrderException($"order with id {orderItemCreationDto.OrderId} could not be found");
        }

        if (order.State != OrderState.Created)
        {
            throw new OrderException($"cannot add product with id '{orderItemCreationDto.ProductId}' " +
                                     $"to order with id '{orderItemCreationDto.OrderId}' " +
                                     $"because the state is '{order.State}' but should be 'created'");
        }

        if (orderItemCreationDto.Quantity == 0)
        {
            orderItemCreationDto.Quantity = 1;
        }

        long result = await _orderItemRepository
            .CreateOrderItem(orderItemCreationDto, cancellationToken);

        if (result == -1)
        {
            throw new OrderException($"product with id '{orderItemCreationDto.ProductId}' could not be added " +
                                     $"to order with id {orderItemCreationDto.OrderId}");
        }

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

        Order? order = await _orderRepository.GetOrderById(orderItemProductRemoveDto.OrderId, cancellationToken);

        if (order == null)
        {
            throw new OrderException($"order with id {orderItemProductRemoveDto.OrderId} could not be found");
        }

        if (order.State != OrderState.Created)
        {
            throw new OrderException($"cannot add product with id '{orderItemProductRemoveDto.ProductId}' " +
                                     $"to order with id '{orderItemProductRemoveDto.OrderId}' " +
                                     $"because the state is '{order.State}' but should be 'created'");
        }

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

        Order? order = await _orderRepository.GetOrderById(orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderException($"order with id {orderId} could not be found");
        }

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

        Order? order = await _orderRepository.GetOrderById(orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderException($"order with id {orderId} could not be found");
        }

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

        Order? order = await _orderRepository.GetOrderById(orderId, cancellationToken);

        if (order == null)
        {
            throw new OrderException($"order with id {orderId} could not be found");
        }

        await _orderRepository
            .UpdateOrderStatus(orderId, OrderState.Cancelled, cancellationToken);

        var orderHistoryData = new OrderHistoryData(
            orderId,
            OrderHistoryItemKind.StateChanged,
            "Order cancelled.");

        await LogOrderHistory(orderHistoryData, cancellationToken);

        scope.Complete();
    }

    public async Task<IAsyncEnumerable<OrderHistoryReturnItemDto>> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        CancellationToken cancellationToken)
    {
        IAsyncEnumerable<OrderHistoryReturnItemDto> orderHistoryReturnItems =
            _orderHistoryRepository
                .GetOrderHistory(orderHistoryItemSearchDto, cancellationToken)
                .Select(_orderHistoryMapper.ToOrderHistoryReturnItemDto);

        if (await orderHistoryReturnItems.CountAsync(cancellationToken) == 0)
        {
            throw new OrderException($"Order with ID {orderHistoryItemSearchDto.OrderId} was not found.");
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        scope.Complete();

        return orderHistoryReturnItems;
    }

    private async Task LogOrderHistory(
        OrderHistoryData orderHistoryData,
        CancellationToken cancellationToken)
    {
        long result = await _orderHistoryRepository
            .CreateOrderHistory(orderHistoryData, cancellationToken);

        if (result == -1L)
        {
            throw new OrderException($"history wasn't recorder for '{orderHistoryData.Kind}' status" +
                                     $"with order id {orderHistoryData.OrderId}");
        }
    }
}