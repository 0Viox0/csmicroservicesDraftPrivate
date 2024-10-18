using Npgsql;
using Task3.Dal.Models;

namespace Task3.Dal.Repositories;

public class ProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task CreateProduct(
        string name,
        decimal productPrice,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into products (product_name, product_price)
                           values (@name, @product_price)
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("@name", name));
        command.Parameters.Add(new NpgsqlParameter("@product_price", productPrice));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProduct(
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken,
        string? nameSubstring = null,
        decimal? maxPrice = null,
        decimal? minPrice = null)
    {
        string sql = """
                     SELECT * 
                     FROM products
                     WHERE
                     (@nameSubstring IS NULL OR product_name ILIKE '%' || @nameSubstring || '%')
                     AND (@minPrice IS NULL OR product_price >= @minPrice)
                     AND (@maxPrice IS NULL OR product_price <= @maxPrice)
                     ORDER BY product_id
                     LIMIT @pageSize OFFSET @pageIndex
                     """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@pageSize", pageSize));
        command.Parameters.Add(new NpgsqlParameter("@pageIndex", pageIndex * pageSize));
        command.Parameters.Add(new NpgsqlParameter("@nameSubstring", nameSubstring));
        command.Parameters.Add(new NpgsqlParameter("@minPrice", minPrice));
        command.Parameters.Add(new NpgsqlParameter("@maxPrice", maxPrice));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        var products = new List<Product>();

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
            });
        }

        return products;
    }
}