using Npgsql;
using System.Text;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Dal.Models;

namespace Task3.Dal.Repositories;

public class OrderItemRepository
{
    public async Task<long> CreateOrderItem(
        OrderItemCreationDto orderItemCreationDto,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        string sql = """
                     insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
                     values (@OrderId, @ProductId, @Quantity, false)
                     returning order_item_id;
                     """;

        using var command = new NpgsqlCommand(sql, transaction.Connection, transaction);
        command.Parameters.AddWithValue("@OrderId", orderItemCreationDto.OrderId);
        command.Parameters.AddWithValue("@ProductId", orderItemCreationDto.ProductId);
        command.Parameters.AddWithValue("@Quantity", orderItemCreationDto.Quantity);

        return (long)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? -1);
    }

    public async Task SoftDeleteItem(
        long orderItemId,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        string sql = """
                     update order_items
                     set order_item_deleted = true
                     where order_item_id = @OrderItemId;
                     """;

        using var command = new NpgsqlCommand(sql, transaction.Connection, transaction);
        command.Parameters.AddWithValue("@OrderItemId", orderItemId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<OrderItem>> SearchOrderItems(
        int pageIndex,
        int pageSize,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken,
        long? orderId = null,
        long? productId = null,
        bool? isDeleted = null,
        int? quantity = null)
    {
        var sql = new StringBuilder("""
                                    select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                                    from order_items
                                    where 1=1
                                    """);

        if (orderId.HasValue)
            sql.Append(" AND order_id = @OrderId");
        if (productId.HasValue)
            sql.Append(" AND product_id = @ProductId");
        if (isDeleted.HasValue)
            sql.Append(" AND order_item_deleted = @IsDeleted");
        if (quantity.HasValue)
            sql.Append(" AND quantity = @Quantity");

        sql.Append(" order by order_item_quantity");
        sql.Append(" limit @PageSize offset @PageIndex");

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        using var command = new NpgsqlCommand(sql.ToString(), transaction.Connection, transaction);
        #pragma warning restore CA2100

        if (orderId.HasValue)
            command.Parameters.AddWithValue("@OrderId", orderId);
        if (productId.HasValue)
            command.Parameters.AddWithValue("@ProductId", productId);
        if (isDeleted.HasValue)
            command.Parameters.AddWithValue("@IsDeleted", isDeleted);
        if (quantity.HasValue)
            command.Parameters.AddWithValue("@Quantity", quantity);

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