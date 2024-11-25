namespace GrpcClientHttpGateway.Models.OrderProcesssingModels;

public class FinishOrderDeliveryRequestBody
{
    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }
}