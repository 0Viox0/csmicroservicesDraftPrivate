namespace Dal.Serializators;

public interface IOrderHistoryDataSerializer
{
    public string Serialize(OrderHistoryData orderHistoryData);

    public OrderHistoryData Deserialize(string payload);
}