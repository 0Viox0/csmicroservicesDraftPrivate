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
        using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        using NpgsqlTransaction transaction =
            await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

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
                cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }
}