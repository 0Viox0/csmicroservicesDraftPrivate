using Dal.Models;
using Dal.Models.Enums;
using Dal.Serializators;
using Npgsql;
using System.Runtime.CompilerServices;

namespace Dal.Repositories;

public class OrderHistoryRepository
{
    private readonly IOrderHistoryDataSerializer _orderHistoryDataSerializer;
    private readonly NpgsqlDataSource _dataSource;

    public OrderHistoryRepository(
        IOrderHistoryDataSerializer orderHistoryDataSerializer,
        NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        _orderHistoryDataSerializer = orderHistoryDataSerializer;
    }

    public async Task<long> CreateOrderHistory(
        OrderHistoryData orderHistoryData,
        CancellationToken cancellationToken)
    {
        string sql = """
                     insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                     VALUES (@OrderId, @CreatedAt, @Kind, @Payload::jsonb)
                     RETURNING order_history_item_id;
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderId", orderHistoryData.OrderId);
        command.Parameters.AddWithValue("@CreatedAt", DateTimeOffset.UtcNow);
        command.Parameters.Add(new NpgsqlParameter("@Kind", orderHistoryData.Kind));
        command.Parameters.AddWithValue("@Payload", _orderHistoryDataSerializer.Serialize(orderHistoryData));

        return (long)(await command.ExecuteScalarAsync(cancellationToken) ?? -1);
    }

    public async IAsyncEnumerable<OrderHistoryItem> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        OrderHistoryItemKind? historyItemKind = null)
    {
        string sql = """
                     select order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                     from order_history
                     where (order_id = @OrderId)
                     and (@OrderHistoryItemKind is null or order_history_item_kind = @OrderHistoryItemKind)
                     order by order_history_item_created_at
                     limit @PageSize offset @Offset
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@OrderId", orderHistoryItemSearchDto.OrderId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@OrderHistoryItemKind",
            Value = historyItemKind ?? (object)DBNull.Value,
            DataTypeName = "order_history_item_kind",
        });

        command.Parameters.Add(new NpgsqlParameter("@PageSize", orderHistoryItemSearchDto.PageSize));
        command.Parameters.Add(new NpgsqlParameter("@Offset", orderHistoryItemSearchDto.PageIndex * orderHistoryItemSearchDto.PageSize));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new OrderHistoryItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2),
                Kind = reader.GetFieldValue<OrderHistoryItemKind>(3),
                Payload = _orderHistoryDataSerializer.Deserialize(reader.GetString(4)),
            };
        }
    }
}