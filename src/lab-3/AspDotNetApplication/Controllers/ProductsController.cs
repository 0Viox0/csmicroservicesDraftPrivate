using Microsoft.AspNetCore.Mvc;
using Task1.Mappers;
using Task1.Models;
using Task3.Bll.Services;

namespace Task1.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly PlMapper _plMapper;

    public ProductsController(
        ProductService productService,
        PlMapper plMapper)
    {
        _productService = productService;
        _plMapper = plMapper;
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
        await _productService.CreateProduct(_plMapper.ToProductCreationDto(productCreationModel), cancellationToken);

        return Created();
    }
}