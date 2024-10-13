using Npgsql;
using System.Data;
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
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            long orderId = await _orderRepository
                .CreateOrder(createdBy, transaction, cancellationToken).ConfigureAwait(false);

            await LogOrderHistory(
                    orderId,
                    OrderHistoryItemKind.Created,
                    $"Order created by {createdBy}",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task AddProductToOrder(OrderItemCreationDto orderItemCreationDto, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            Order order =
                (await _orderRepository
                    .SearchOrders(0, 1, transaction, cancellationToken, [orderItemCreationDto.OrderId])
                    .ConfigureAwait(false))
                .First();

            if (order.State != OrderState.Created)
                return;

            await _orderItemRepository
                .CreateOrderItem(orderItemCreationDto, transaction, cancellationToken)
                .ConfigureAwait(false);

            await LogOrderHistory(
                    orderItemCreationDto.OrderId,
                    OrderHistoryItemKind.ItemAdded,
                    $"Product {orderItemCreationDto.ProductId} added to order {orderItemCreationDto.OrderId}. Quantity: {orderItemCreationDto.Quantity}",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task RemoveProductFromOrder(OrderItemRemoveDto orderItemProductRemoveDto, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            Order order
                = (await _orderRepository
                    .SearchOrders(0, 1, transaction, cancellationToken, [orderItemProductRemoveDto.OrderId])
                    .ConfigureAwait(false))
                .First();

            if (order is not { State: OrderState.Created })
                return;

            OrderItem orderItem =
                (await _orderItemRepository
                    .SearchOrderItems(0, 1, transaction, cancellationToken, orderId: orderItemProductRemoveDto.OrderId, productId: orderItemProductRemoveDto.ProductId)
                    .ConfigureAwait(false)).First();

            await _orderItemRepository
                .SoftDeleteItem(orderItem.Id, transaction, cancellationToken)
                .ConfigureAwait(false);

            await LogOrderHistory(
                    orderItem.OrderId,
                    OrderHistoryItemKind.ItemRemoved,
                    $"Product removed from order {orderItem.OrderId}. Item ID: {orderItemProductRemoveDto.ProductId}",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task TransferOrderToProcessing(long orderId, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Processing, transaction, cancellationToken)
                .ConfigureAwait(false);

            await LogOrderHistory(
                    orderId,
                    OrderHistoryItemKind.StateChanged,
                    "Order transferred to processing.",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task FulfillOrder(long orderId, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Completed, transaction, cancellationToken)
                .ConfigureAwait(false);

            await LogOrderHistory(
                    orderId,
                    OrderHistoryItemKind.StateChanged,
                    "Order fulfilled.",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task CancelOrder(long orderId, CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _orderRepository
                .UpdateOrderStatus(orderId, OrderState.Cancelled, transaction, cancellationToken)
                .ConfigureAwait(false);

            await LogOrderHistory(
                    orderId,
                    OrderHistoryItemKind.StateChanged,
                    "Order cancelled.",
                    transaction,
                    cancellationToken)
                .ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<IEnumerable<OrderHistoryReturnItemDto>> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        CancellationToken cancellationToken)
    {
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var orderHistoryReturnItems =
                (await _orderHistoryRepository
                    .GetOrderHistory(orderHistoryItemSearchDto, transaction, cancellationToken)
                    .ConfigureAwait(false))
                .Select(_orderHistoryMapper.ToOrderHistoryReturnItemDto)
                .ToList();

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return orderHistoryReturnItems;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Enumerable.Empty<OrderHistoryReturnItemDto>();
        }
    }

    private async Task LogOrderHistory(
        long orderId,
        OrderHistoryItemKind kind,
        string payload,
        NpgsqlTransaction transaction,
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
            .CreateOrderHistory(orderId, kind, jsonFormattedPayload, transaction, cancellationToken)
            .ConfigureAwait(false);
    }
}