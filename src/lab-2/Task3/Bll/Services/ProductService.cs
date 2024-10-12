using Task3.Bll.Dtos.ProductDtos;
using Task3.Dal.Repositories;

namespace Task3.Bll.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task CreateProduct(
        ProductCreationDto productCreationDto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(productCreationDto.Name))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(productCreationDto));
        }

        if (productCreationDto.Price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productCreationDto), "Product price must be greater than zero.");
        }

        await _productRepository.CreateProduct(
            productCreationDto.Name,
            productCreationDto.Price,
            cancellationToken).ConfigureAwait(false);
    }
}