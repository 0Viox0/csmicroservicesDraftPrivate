using GrpcClientHttpGateway.Mappers;
using GrpcClientHttpGateway.Models.OrderProcesssingModels;
using Microsoft.AspNetCore.Mvc;
using Orders.ProcessingService.Contracts;

namespace GrpcClientHttpGateway.Controllers;

[ApiController]
[Route("orders")]
public class OrderProcessingController : ControllerBase
{
    private readonly OrderService.OrderServiceClient _orderProcessingServiceClient;
    private readonly ModelToOrderServiceGrpcMapper _mapper;

    public OrderProcessingController(
        OrderService.OrderServiceClient orderProcessingServiceClient,
        ModelToOrderServiceGrpcMapper mapper)
    {
        _orderProcessingServiceClient = orderProcessingServiceClient;
        _mapper = mapper;
    }

    [HttpPatch("{orderId}/approval")]
    public async Task<IActionResult> ApproveOrder(
        long orderId,
        [FromBody] ApproveOrderRequestBody requestBody)
    {
        await _orderProcessingServiceClient.ApproveOrderAsync(
            _mapper.ToApproveOrderRequest(orderId, requestBody));

        return Ok();
    }

    [HttpPost("{orderId}/packing")]
    public async Task<IActionResult> StartPacking(
        long orderId,
        [FromBody] StartOrderPackingRequestBody requestBody)
    {
        await _orderProcessingServiceClient.StartOrderPackingAsync(
            _mapper.ToStartOrderPackingRequest(orderId, requestBody));

        return Ok();
    }

    [HttpPatch("{orderId}/packing")]
    public async Task<IActionResult> FinishPacking(
        long orderId,
        [FromBody] FinishOrderPackingRequestBody requestBody)
    {
        await _orderProcessingServiceClient.FinishOrderPackingAsync(
            _mapper.ToFinishOrderPackingRequest(orderId, requestBody));

        return Ok();
    }

    [HttpPost("{orderId}/delivery")]
    public async Task<IActionResult> StartDelivery(
        long orderId,
        [FromBody] StartOrderDeliveryRequestBody requestBody)
    {
        await _orderProcessingServiceClient.StartOrderDeliveryAsync(
            _mapper.ToStartOrderDeliveryRequest(orderId, requestBody));

        return Ok();
    }

    [HttpPatch("{orderId}/delivery")]
    public async Task<IActionResult> FinishDelivery(
        long orderId,
        [FromBody] FinishOrderDeliveryRequestBody requestBody)
    {
        await _orderProcessingServiceClient.FinishOrderDeliveryAsync(
            _mapper.ToFinishOrderDeliveryRequest(orderId, requestBody));

        return Ok();
    }
}