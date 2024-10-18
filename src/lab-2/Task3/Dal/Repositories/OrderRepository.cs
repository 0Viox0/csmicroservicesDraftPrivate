using Npgsql;
using System.Text;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;

namespace Task3.Dal.Repositories;

public class OrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateOrder(
        string createdBy,
        CancellationToken cancellationToken)
    {
        string sql = """
                     insert into orders (order_state, order_created_at, order_created_by)
                     values ('created', NOW(), @createdBy)
                     RETURNING order_id
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("@createdBy", createdBy));

        return (long)(await command.ExecuteScalarAsync(cancellationToken) ?? -1L);
    }

    public async Task UpdateOrderStatus(
        long orderId,
        OrderState state,
        CancellationToken cancellationToken)
    {
        string sql = """
                     UPDATE orders SET
                     order_state = @newStatus::order_state
                     WHERE order_id = @orderId
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("@newStatus", state.ToString().ToLowerInvariant()));
        command.Parameters.Add(new NpgsqlParameter("@orderId", orderId));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> SearchOrders(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken,
        long[]? orderIds = null,
        OrderState? state = null,
        string? author = null)
    {
        var sql = new StringBuilder(@"
                          SELECT order_id, order_state, order_created_at, order_created_by
                          FROM orders
                          WHERE 1=1");

        if (orderIds is not null && orderIds.Length > 0)
            sql.Append(" AND order_id = ANY(@orderIds)");

        if (state is not null)
            sql.Append(" AND order_state = @newStatus::order_state");

        if (!string.IsNullOrEmpty(author))
            sql.Append(" AND order_created_by = @author");

        sql.Append(@"
                order by order_created_at desc
                limit @pageSize offset @offset;");

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql.ToString(), connection);
        #pragma warning restore CA2100

        if (orderIds != null && orderIds.Length > 0)
        {
            command.Parameters.AddWithValue("@orderIds", orderIds);
        }

        if (state is not null)
        {
            string stateString = state.ToString()?.ToLowerInvariant() ?? "this will never happen bad analyzer 0_0";
            command.Parameters.AddWithValue("@newStatus", stateString);
        }

        if (!string.IsNullOrEmpty(author))
        {
            command.Parameters.AddWithValue("@author", author);
        }

        command.Parameters.AddWithValue("@offset", pageIndex * pageSize);
        command.Parameters.AddWithValue("@pageSize", pageSize);

        var orders = new List<Order>();

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new Order
            {
                Id = reader.GetInt32(0),
                State = (OrderState)Enum.Parse(typeof(OrderState), reader.GetString(1), true),
                CreatedAt = reader.GetDateTime(2),
                CreatedBy = reader.GetString(3),
            });
        }

        return orders;
    }
}