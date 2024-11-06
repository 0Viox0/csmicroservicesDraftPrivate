using Npgsql;
using System.Runtime.CompilerServices;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Dal.Models;

namespace Task3.Dal.Repositories;

public class OrderItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateOrderItem(
        OrderItemCreationDto orderItemCreationDto,
        CancellationToken cancellationToken)
    {
        string sql = """
                     insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
                     values (@OrderId, @ProductId, @Quantity, false)
                     returning order_item_id;
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderId", orderItemCreationDto.OrderId);
        command.Parameters.AddWithValue("@ProductId", orderItemCreationDto.ProductId);
        command.Parameters.AddWithValue("@Quantity", orderItemCreationDto.Quantity);

        return (long)(await command.ExecuteScalarAsync(cancellationToken) ?? -1);
    }

    public async Task SoftDeleteItem(
        long orderItemId,
        CancellationToken cancellationToken)
    {
        string sql = """
                     update order_items
                     set order_item_deleted = true
                     where order_item_id = @OrderItemId;
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderItemId", orderItemId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<OrderItem> SearchOrderItems(
        int pageIndex,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        long? orderId = null,
        long? productId = null,
        bool? isDeleted = null,
        int? quantity = null)
    {
        string sql = """
                  select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                  from order_items
                  where (@OrderId is null or order_id = @OrderId)
                  and (@ProductId is null or product_id = @ProductId)
                  and (@IsDeleted is null or order_item_deleted = @IsDeleted)
                  and (@Quantity is null or order_item_quantity = @Quantity)
                  order by order_item_quantity
                  limit @PageSize offset @PageIndex
                  """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@OrderId", orderId));
        command.Parameters.Add(new NpgsqlParameter("@ProductId", productId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@IsDeleted",
            Value = isDeleted ?? (object)DBNull.Value,
            DataTypeName = "boolean",
        });
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Quantity",
            Value = quantity ?? (object)DBNull.Value,
            DataTypeName = "integer",
        });
        command.Parameters.Add(new NpgsqlParameter("@PageSize", pageSize));
        command.Parameters.Add(new NpgsqlParameter("@PageIndex", pageIndex));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderItem
            {
                Id = reader.GetInt32(0),
                OrderId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ItemQuantity = reader.GetInt32(3),
                IsOrderItemDeleted = reader.GetBoolean(4),
            };
        }
    }
}