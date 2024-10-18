using Npgsql;
using System.Runtime.CompilerServices;
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
                     order_state = @newStatus
                     WHERE order_id = @orderId
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("@newStatus", state));
        command.Parameters.Add(new NpgsqlParameter("@orderId", orderId));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Order> SearchOrders(
        int pageIndex,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        long[]? orderIds = null,
        OrderState? state = null,
        string? author = null)
    {
        string sql = """
                     SELECT order_id, order_state, order_created_at, order_created_by
                     FROM orders
                     WHERE (@orderIds is null or order_id = ANY(@orderIds))
                     AND (@newStatus is null or order_state = @newStatus)
                     AND (@author is null or order_created_by = @author)
                     order by order_created_at desc
                     limit @pageSize offset @offset;
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@orderIds", orderIds));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@newStatus",
            Value = state ?? (object)DBNull.Value,
            DataTypeName = "order_state",
        });
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@author",
            Value = author ?? (object)DBNull.Value,
            DataTypeName = "text",
        });
        command.Parameters.Add(new NpgsqlParameter("@offset", pageIndex * pageSize));
        command.Parameters.Add(new NpgsqlParameter("@pageSize", pageSize));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Order
            {
                Id = reader.GetInt32(0),
                State = reader.GetFieldValue<OrderState>(1),
                CreatedAt = reader.GetDateTime(2),
                CreatedBy = reader.GetString(3),
            };
        }
    }
}