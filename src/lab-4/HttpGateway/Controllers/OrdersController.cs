using GrpcClientHttpGateway.Mappers;
using GrpcClientHttpGateway.Models;
using GrpcServer;
using Microsoft.AspNetCore.Mvc;

namespace GrpcClientHttpGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersService.OrdersServiceClient _orderServiceClient;
    private readonly GrpcModelMapper _mapper;

    public OrdersController(
        OrdersService.OrdersServiceClient orderServiceClient,
        GrpcModelMapper mapper)
    {
        _orderServiceClient = orderServiceClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /orders
    ///     {
    ///         "createdBy": "John"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the ID of the newly created order.</response>
    /// <response code="400">If the request is invalid or the order cannot be created.</response>
    /// <returns>The ID of the newly created order.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(long))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder(
        CreatedByModel createdByModel,
        CancellationToken cancellationToken)
    {
        if (createdByModel.CreatedBy is null)
        {
            return BadRequest();
        }

        CreateOrderResponse orderId =
            await _orderServiceClient.CreateOrderAsync(
                _mapper.ToCreateOrderRequest(createdByModel),
                cancellationToken: cancellationToken);

        return Created($"orders/{orderId}", orderId.OrderId);
    }

    /// <summary>
    /// Adds a product to an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order to add the product to.</param>
    /// <param name="productId">The ID of the product to add.</param>
    /// <param name="quantity">The quantity of the product to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /orders/1/product/2?quantity=5
    ///
    /// </remarks>
    /// <response code="200">Indicates that the product was successfully added to the order.</response>
    /// <response code="404">If the order or product is not found.</response>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{orderId}/product/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductToOrder(
        long orderId,
        long productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        await _orderServiceClient.AddProductToOrderAsync(
            _mapper.ToAddProductToOrderRequest(orderId, productId, quantity),
            cancellationToken: cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Removes a product from an existing order.
    /// </summary>
    /// <param name="orderId">The ID of the order from which the product will be removed.</param>
    /// <param name="productId">The ID of the product to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /orders/1/product/2
    ///
    /// </remarks>
    /// <response code="200">Indicates that the product was successfully removed from the order.</response>
    /// <response code="404">If the order or product is not found.</response>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpDelete("{orderId}/product/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProductFromOrder(
        long orderId,
        long productId,
        CancellationToken cancellationToken)
    {
        await _orderServiceClient.RemoveProductFromOrderAsync(
            _mapper.ToRemoveProductFromOrderRequest(orderId, productId),
            cancellationToken: cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Transfers an order to processing.
    /// </summary>
    /// <param name="orderId">The ID of the order to be transferred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /orders/1/process
    ///
    /// </remarks>
    /// <response code="200">Indicates that the order has been successfully transferred to processing.</response>
    /// <response code="404">If the order is not found.</response>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{orderId}/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferOrderToProcessing(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderServiceClient.TransferOrderToProcessingAsync(
            new TransferOrderToProcessingRequest { OrderId = orderId },
            cancellationToken: cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Fulfills an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to fulfill.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /orders/1/fulfill
    ///
    /// </remarks>
    /// <response code="204">Indicates that the order has been successfully fulfilled.</response>
    /// <response code="404">If the order is not found.</response>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{orderId}/fulfill")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FulfillOrder(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderServiceClient.FulfillOrderAsync(
            new FulfillOrderRequest { OrderId = orderId },
            cancellationToken: cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Cancels an order.
    /// </summary>
    /// <param name="orderId">The ID of the order to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /orders/1/cancel
    ///
    /// </remarks>
    /// <response code="204">Indicates that the order has been successfully canceled.</response>
    /// <response code="404">If the order is not found.</response>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpPost("{orderId}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderServiceClient.CancelOrderAsync(
            new CancelOrderRequest { OrderId = orderId },
            cancellationToken: cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gets the order history for a specific order.
    /// </summary>
    /// <param name="orderId">The ID of the order for which to retrieve history.</param>
    /// <param name="pageIndex">The index of the page to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /orders/1/history?pageIndex=0&amp;pageSize=10
    ///
    /// </remarks>
    /// <response code="200">Returns the order history items.</response>
    /// <response code="404">If the order is not found.</response>
    /// <returns>A list of order history items.</returns>
    [HttpGet("{orderId}/history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GetOrderHistoryResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderHistory(
        long orderId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        GetOrderHistoryResponse result = await _orderServiceClient.GetOrderHistoryAsync(
            _mapper.ToGetOrderHistoryRequest(orderId, pageIndex, pageSize),
            cancellationToken: cancellationToken);

        var orderHistoryItems = result.OrderHistory
            .Select(_mapper.ToOrderHistoryItemReturnModel)
            .ToList();

        return Ok(orderHistoryItems);
    }
}