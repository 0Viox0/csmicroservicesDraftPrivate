namespace GrpcClientHttpGateway.Models.OrderProcesssingModels;

public class ApproveOrderRequestBody
{
    public bool IsApproved { get; set; }

    public string? ApprovedBy { get; set; }

    public string? FailureReason { get; set; }
}