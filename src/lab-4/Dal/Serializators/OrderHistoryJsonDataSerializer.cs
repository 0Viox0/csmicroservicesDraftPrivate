using System.Text.Json;

namespace Dal.Serializators;

public class OrderHistoryJsonDataSerializer : IOrderHistoryDataSerializer
{
    public string Serialize(OrderHistoryData orderHistoryData)
    {
        return JsonSerializer.Serialize(orderHistoryData);
    }

    public OrderHistoryData Deserialize(string payload)
    {
        return JsonSerializer.Deserialize<OrderHistoryData>(payload)
               ?? new OrderHistoryData(default, default, string.Empty);
    }
}