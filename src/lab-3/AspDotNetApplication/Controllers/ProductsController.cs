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

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] ProductCreationModel productCreationModel,
        CancellationToken cancellationToken)
    {
        await _productService.CreateProduct(_plMapper.ToProductCreationDto(productCreationModel), cancellationToken);

        return Created();
    }
}