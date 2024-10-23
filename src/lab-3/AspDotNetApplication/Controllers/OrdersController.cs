using Microsoft.AspNetCore.Mvc;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Services;

namespace Task1.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] string createdBy,
        CancellationToken cancellationToken)
    {
        long orderId = await _orderService.CreateOrder(createdBy, cancellationToken);

        return Created($"orders/{orderId}", orderId);
    }

    [HttpPost("{orderId}/product/{productid}")]
    public async Task<IActionResult> AddProductToOrder(
        long orderId,
        long productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var orderItemCreationDto = new OrderItemCreationDto
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };

        await _orderService.AddProductToOrder(orderItemCreationDto, cancellationToken);

        return Ok();
    }

    [HttpDelete("{orderId}/product/{productId}")]
    public async Task<IActionResult> RemoveProductFromOrder(
        long orderId,
        long productId,
        CancellationToken cancellationToken)
    {
        var orderItemRemoveDto = new OrderItemRemoveDto
        {
            OrderId = orderId,
            ProductId = productId,
        };

        await _orderService.RemoveProductFromOrder(orderItemRemoveDto, cancellationToken);
        return Ok();
    }

    [HttpPost("{orderId}/process")]
    public async Task<IActionResult> TransferOrderToProcessing(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.TransferOrderToProcessing(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("{orderId}/fulfill")]
    public async Task<IActionResult> FulfillOrder(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.FulfillOrder(orderId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(
        long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.CancelOrder(orderId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{orderId}/history")]
    public async Task<IActionResult> GetOrderHistory(
        long orderId,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var searchDto = new OrderHistoryItemSearchDto
        {
            OrderId = orderId,
            PageIndex = pageIndex,
            PageSize = pageSize,
        };

        IAsyncEnumerable<OrderHistoryReturnItemDto> history =
            _orderService.GetOrderHistory(searchDto, cancellationToken);

        return Ok(await history.ToListAsync(cancellationToken));
    }
}