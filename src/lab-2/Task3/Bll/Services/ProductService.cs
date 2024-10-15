using Npgsql;
using Task3.Bll.Dtos.ProductDtos;
using Task3.Dal.Repositories;

namespace Task3.Bll.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly NpgsqlDataSource _dataSource;

    public ProductService(
        ProductRepository productRepository,
        NpgsqlDataSource dataSource)
    {
        _productRepository = productRepository;
        _dataSource = dataSource;
    }

    public async Task CreateProduct(
        ProductCreationDto productCreationDto,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);
        await using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (string.IsNullOrWhiteSpace(productCreationDto.Name))
                throw new ArgumentException("Product name cannot be empty.", nameof(productCreationDto));

            if (productCreationDto.Price <= 0)
                throw new ArgumentOutOfRangeException(nameof(productCreationDto), "Product price must be greater than zero.");

            await _productRepository.CreateProduct(
                productCreationDto.Name,
                productCreationDto.Price,
                transaction,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}