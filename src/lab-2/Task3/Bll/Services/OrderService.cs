using Npgsql;
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
    private readonly NpgsqlDataSource _dataSource;

    public OrderService(
        OrderRepository orderRepository,
        OrderItemRepository orderItemRepository,
        OrderHistoryRepository orderHistoryRepository,
        OrderHistoryMapper orderHistoryMapper,
        NpgsqlDataSource dataSource)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _orderHistoryMapper = orderHistoryMapper;
        _dataSource = dataSource;
    }

    public async Task<long> CreateOrder(string createdBy, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            long orderId = await _orderRepository
                .CreateOrder(createdBy, transaction, cancellationToken);

            var orderHistoryData =
                    new OrderHistoryData(
                        orderId,
                        OrderHistoryItemKind.Created,
                        $"Order created by {createdBy}");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task AddProductToOrder(OrderItemCreationDto orderItemCreationDto, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            Order order =
                (await _orderRepository
                    .SearchOrders(0, 1, transaction, cancellationToken, [orderItemCreationDto.OrderId]))
                .First();

            if (order.State != OrderState.Created)
                return;

            await _orderItemRepository
                .CreateOrderItem(orderItemCreationDto, transaction, cancellationToken);

            var orderHistoryData = new OrderHistoryData(
                orderItemCreationDto.OrderId,
                OrderHistoryItemKind.ItemAdded,
                $"Product {orderItemCreationDto.ProductId} added to order {orderItemCreationDto.OrderId}. Quantity: {orderItemCreationDto.Quantity}");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task RemoveProductFromOrder(OrderItemRemoveDto orderItemProductRemoveDto, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            Order order
                = (await _orderRepository
                    .SearchOrders(0, 1, transaction, cancellationToken, [orderItemProductRemoveDto.OrderId]))
                .First();

            if (order is not { State: OrderState.Created })
                return;

            OrderItem orderItem =
                (await _orderItemRepository
                    .SearchOrderItems(0, 1, transaction, cancellationToken, orderId: orderItemProductRemoveDto.OrderId, productId: orderItemProductRemoveDto.ProductId))
                .First();

            await _orderItemRepository
                .SoftDeleteItem(orderItem.Id, transaction, cancellationToken);

            var orderHistoryData = new OrderHistoryData(
                orderItem.OrderId,
                OrderHistoryItemKind.ItemRemoved,
                $"Product removed from order {orderItem.OrderId}. Item ID: {orderItemProductRemoveDto.ProductId}");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task TransferOrderToProcessing(long orderId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Processing, transaction, cancellationToken);

            var orderHistoryData = new OrderHistoryData(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order transferred to processing.");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task FulfillOrder(long orderId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Completed, transaction, cancellationToken);

            var orderHistoryData = new OrderHistoryData(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order fulfilled.");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task CancelOrder(long orderId, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Cancelled, transaction, cancellationToken);

            var orderHistoryData = new OrderHistoryData(
                orderId,
                OrderHistoryItemKind.StateChanged,
                "Order cancelled.");

            await LogOrderHistory(orderHistoryData, transaction, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<OrderHistoryReturnItemDto>> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderHistoryReturnItems =
                (await _orderHistoryRepository
                    .GetOrderHistory(orderHistoryItemSearchDto, transaction, cancellationToken))
                .Select(_orderHistoryMapper.ToOrderHistoryReturnItemDto)
                .ToList();

            await transaction.CommitAsync(cancellationToken);

            return orderHistoryReturnItems;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return Enumerable.Empty<OrderHistoryReturnItemDto>();
        }
    }

    private async Task LogOrderHistory(
        OrderHistoryData orderHistoryData,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await _orderHistoryRepository
            .CreateOrderHistory(orderHistoryData, transaction, cancellationToken);
    }
}