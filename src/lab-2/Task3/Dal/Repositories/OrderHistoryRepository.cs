using Npgsql;
using System.Text;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Extensions;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;
using Task3.Dal.Serializators;

namespace Task3.Dal.Repositories;

public class OrderHistoryRepository
{
    private readonly IOrderHistoryDataSerializer _orderHistoryDataSerializer;

    public OrderHistoryRepository(IOrderHistoryDataSerializer orderHistoryDataSerializer)
    {
        _orderHistoryDataSerializer = orderHistoryDataSerializer;
    }

    public async Task<long> CreateOrderHistory(
        OrderHistoryData orderHistoryData,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = @"
                    insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                    VALUES (@OrderId, @CreatedAt, @Kind::order_history_item_kind, @Payload::jsonb)
                    RETURNING order_history_item_id;";

        using var command = new NpgsqlCommand(sql, transaction.Connection, transaction);
        command.Parameters.AddWithValue("@OrderId", orderHistoryData.OrderId);
        command.Parameters.AddWithValue("@CreatedAt", DateTimeOffset.UtcNow);
        command.Parameters.AddWithValue("@Kind", orderHistoryData.Kind.ToString().FromPascalToSnakeCase());
        command.Parameters.AddWithValue("@Payload", _orderHistoryDataSerializer.Serialize(orderHistoryData));

        return (long)(await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) ?? -1);
    }

    public async Task<IEnumerable<OrderHistoryItem>> GetOrderHistory(
        OrderHistoryItemSearchDto orderHistoryItemSearchDto,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken,
        OrderHistoryItemKind? historyItemKind = null)
    {
        var sql = new StringBuilder("""
                                    select order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                                    from order_history
                                    where 1=1
                                    """);
        sql.Append(" and order_history.order_id = @OrderId");

        if (historyItemKind.HasValue)
            sql.Append(" and order_history_item_kind = @OrderHistoryItemKind::order_history_item_kind");

        sql.Append("""
                    order by order_history_item_created_at asc
                   limit @PageSize offset @Offset;
                   """);

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        using var command = new NpgsqlCommand(sql.ToString(), transaction.Connection, transaction);
        #pragma warning restore CA2100

        command.Parameters.AddWithValue("@OrderId", orderHistoryItemSearchDto.OrderId);

        if (historyItemKind.HasValue)
        {
            command.Parameters.AddWithValue(
                "@OrderHistoryItemKind",
                historyItemKind.Value.ToString().ToLowerInvariant());
        }

        command.Parameters.AddWithValue("@PageSize", orderHistoryItemSearchDto.PageSize);
        command.Parameters.AddWithValue("@Offset", orderHistoryItemSearchDto.PageIndex * orderHistoryItemSearchDto.PageSize);

        using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var orderHistories = new List<OrderHistoryItem>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            orderHistories.Add(new OrderHistoryItem
            {
                Id = reader.GetInt64(0),
                OrderId = reader.GetInt64(1),
                CreatedAt = reader.GetDateTime(2),
                Kind = (OrderHistoryItemKind)Enum.Parse(
                    typeof(OrderHistoryItemKind),
                    reader.GetString(3).FromSnakeToPascalCase(),
                    true),
                Payload = _orderHistoryDataSerializer.Deserialize(reader.GetString(4)),
            });
        }

        return orderHistories;
    }
}