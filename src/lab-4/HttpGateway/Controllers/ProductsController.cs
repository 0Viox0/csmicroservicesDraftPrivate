using GrpcClientHttpGateway.Mappers;
using GrpcClientHttpGateway.Models;
using GrpcServer;
using Microsoft.AspNetCore.Mvc;

namespace GrpcClientHttpGateway.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ProductsService.ProductsServiceClient _productServiceClient;
    private readonly GrpcModelMapper _mapper;

    public ProductsController(
        GrpcModelMapper mapper,
        ProductsService.ProductsServiceClient productServiceClient)
    {
        _mapper = mapper;
        _productServiceClient = productServiceClient;
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="productCreationModel">The model containing the details of the product to be created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /products
    ///     {
    ///         "name": "Sample Product",
    ///         "description": "This is a sample product.",
    ///         "price": 19.99,
    ///         "stock": 100
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the location of the created product.</response>
    /// <response code="400">If the product details are invalid.</response>
    /// <returns>An action result indicating the outcome of the operation.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] ProductCreationModel productCreationModel,
        CancellationToken cancellationToken)
    {
        await _productServiceClient.CreateProductAsync(
            _mapper.ToCreateProductRequest(productCreationModel),
            cancellationToken: cancellationToken);

        return Created();
    }
}