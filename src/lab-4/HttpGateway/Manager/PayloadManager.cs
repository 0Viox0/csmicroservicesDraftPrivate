using GrpcServer;

namespace GrpcClientHttpGateway.Manager;

public class PayloadManager
{
    public object? GetPayload(OrderHistoryData payload)
    {
        object?[] payloads =
        {
            payload.CreatedData,
            payload.ItemAddedData,
            payload.ItemRemovedData,
            payload.StateChangedData,
        };

        return payloads.FirstOrDefault(data => data != null);
    }
}