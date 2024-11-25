namespace GrpcClientHttpGateway.Models.OrderProcesssingModels;

public class FinishOrderPackingRequestBody
{
    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }
}