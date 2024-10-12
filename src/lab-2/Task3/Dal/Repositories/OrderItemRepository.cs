using Npgsql;
using System.Text;
using Task3.Dal.Models;

namespace Task3.Dal.Repositories;

public class OrderItemRepository
{
    private readonly NpgsqlDataSource _datasource;

    public OrderItemRepository(NpgsqlDataSource datasource)
    {
        _datasource = datasource;
    }

    public async Task<long> CreateOrderItem(long orderId, int productId, int quantity, CancellationToken cancellationToken)
    {
        string sql = """
                     insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
                     values (@OrderId, @ProductId, @Quantity, false)
                     returning order_item_id;
                     """;

        using NpgsqlConnection connection = await _datasource.OpenConnectionAsync().ConfigureAwait(false);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@ProductId", productId);
        command.Parameters.AddWithValue("@Quantity", quantity);

        return (long)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? -1);
    }

    public async Task SoftDeleteItem(long orderItemId, CancellationToken cancellationToken)
    {
        string sql = """
                     update order_items
                     set order_item_deleted = true
                     where order_item_id = @OrderItemId;
                     """;

        using NpgsqlConnection connection = await _datasource.OpenConnectionAsync().ConfigureAwait(false);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderItemId", orderItemId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrderItem>> SearchOrderItems(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken,
        string? orderId = null,
        int? productId = null,
        bool? isDeleted = null,
        int? quantity = null)
    {
        var sql = new StringBuilder("""
                                    select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                                    from order_items
                                    where 1=1
                                    """);

        if (!string.IsNullOrEmpty(orderId))
        {
            sql.Append(" AND order_id = @OrderId");
        }

        if (productId.HasValue)
        {
            sql.Append(" AND product_id = @ProductId");
        }

        if (isDeleted.HasValue)
        {
            sql.Append(" AND order_item_deleted = @IsDeleted");
        }

        if (quantity.HasValue)
        {
            sql.Append(" AND quantity = @Quantity");
        }

        sql.Append(" order by order_item_quantity");
        sql.Append(" limit @PageSize offset @PageIndex");

        using NpgsqlConnection connection
            = await _datasource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        using var command = new NpgsqlCommand(sql.ToString(), connection);
        #pragma warning restore CA2100

        if (!string.IsNullOrEmpty(orderId))
        {
            command.Parameters.AddWithValue("@OrderId", orderId);
        }

        if (productId.HasValue)
        {
            command.Parameters.AddWithValue("@ProductId", productId);
        }

        if (isDeleted.HasValue)
        {
            command.Parameters.AddWithValue("@IsDeleted", isDeleted);
        }

        if (quantity.HasValue)
        {
            command.Parameters.AddWithValue("@Quantity", quantity);
        }

        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@PageIndex", pageIndex);

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var orderItems = new List<OrderItem>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            orderItems.Add(new OrderItem
            {
                Id = reader.GetInt32(0),
                OrderId = reader.GetInt32(1),
                ProductId = reader.GetInt32(2),
                ItemQuantity = reader.GetInt32(3),
                IsOrderItemDeleted = reader.GetBoolean(4),
            });
        }

        return orderItems;
    }
}