using GrpcClientHttpGateway.Models.OrderProcesssingModels;
using Orders.ProcessingService.Contracts;

namespace GrpcClientHttpGateway.Mappers;

public class ModelToOrderServiceGrpcMapper
{
    public ApproveOrderRequest ToApproveOrderRequest(
        long id,
        ApproveOrderRequestBody requestBody)
    {
        return new ApproveOrderRequest
        {
            OrderId = id,
            IsApproved = requestBody.IsApproved,
            ApprovedBy = requestBody.ApprovedBy,
            FailureReason = requestBody.FailureReason,
        };
    }

    public StartOrderPackingRequest ToStartOrderPackingRequest(
        long id,
        StartOrderPackingRequestBody requestBody)
    {
        return new StartOrderPackingRequest
        {
            OrderId = id,
            PackingBy = requestBody.PackingBy,
        };
    }

    public FinishOrderPackingRequest ToFinishOrderPackingRequest(
        long id,
        FinishOrderPackingRequestBody requestBody)
    {
        return new FinishOrderPackingRequest
        {
            OrderId = id,
            IsSuccessful = requestBody.IsSuccessful,
            FailureReason = requestBody.FailureReason,
        };
    }

    public StartOrderDeliveryRequest ToStartOrderDeliveryRequest(
        long id,
        StartOrderDeliveryRequestBody requestBody)
    {
        return new StartOrderDeliveryRequest
        {
            OrderId = id,
            DeliveredBy = requestBody.DeliveredBy,
        };
    }

    public FinishOrderDeliveryRequest ToFinishOrderDeliveryRequest(
        long id,
        FinishOrderDeliveryRequestBody requestBody)
    {
        return new FinishOrderDeliveryRequest
        {
            OrderId = id,
            IsSuccessful = requestBody.IsSuccessful,
            FailureReason = requestBody.FailureReason,
        };
    }
}