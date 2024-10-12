using Npgsql;
using System.Text;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;

namespace Task3.Dal.Repositories;

public class OrderHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> CreateOrderHistory(
        long orderId,
        OrderHistoryItemKind orderHistoryItemKind,
        string payload,
        CancellationToken cancellationToken)
    {
        const string sql = @"
                    insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                    VALUES (@OrderId, @CreatedAt, @Kind::order_history_item_kind, @Payload::jsonb)
                    RETURNING order_history_item_id;";

        using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("OrderId", orderId);
        command.Parameters.AddWithValue("CreatedAt", DateTimeOffset.UtcNow);
        command.Parameters.AddWithValue("Kind", orderHistoryItemKind.ToString().ToLowerInvariant());
        command.Parameters.AddWithValue("Payload", payload);

        return (long)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? -1);
    }

    public async Task<IEnumerable<OrderHistoryItem>> GetOrderHistory(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken,
        OrderHistoryItemKind? historyItemKind = null,
        long? orderId = null)
    {
        var sql = new StringBuilder("""
                                    select order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                                    from order_history
                                    where 1=1
                                    """);
        if (orderId.HasValue)
        {
            sql.Append(" and order_history.order_id = @OrderId");
        }

        if (historyItemKind.HasValue)
        {
            sql.Append(" and order_history_item_kind = @OrderHistoryItemKind::order_history_item_kind");
        }

        sql.Append("""
                    order by order_history_item_created_at desc
                   limit @PageSize offset @Offset;
                   """);

        using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        using var command = new NpgsqlCommand(sql.ToString(), connection);
        #pragma warning restore CA2100

        if (orderId.HasValue)
        {
            command.Parameters.AddWithValue("@OrderId", orderId);
        }

        if (historyItemKind.HasValue)
        {
            command.Parameters.AddWithValue(
                "@OrderHistoryItemKind",
                historyItemKind.Value.ToString().ToLowerInvariant());
        }

        command.Parameters.AddWithValue("PageSize", pageSize);
        command.Parameters.AddWithValue("Offset", pageIndex * pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var orderHistories = new List<OrderHistoryItem>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            orderHistories.Add(new OrderHistoryItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2),
                Kind = (OrderHistoryItemKind)Enum.Parse(typeof(OrderHistoryItemKind), reader.GetString(3), true),
                Payload = reader.GetString(4),
            });
        }

        return orderHistories;
    }
}