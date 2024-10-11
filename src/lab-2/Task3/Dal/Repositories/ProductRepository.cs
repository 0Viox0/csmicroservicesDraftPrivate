using Npgsql;
using System.Text;
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

        using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("name", name),
                new NpgsqlParameter("product_price", productPrice),
            },
        };

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Product>> SearchProduct(
        int pageIndex,
        int pageSize,
        string? nameSubstring,
        decimal? maxPrice,
        decimal? minPrice,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder("SELECT * FROM products WHERE 1=1");

        if (!string.IsNullOrEmpty(nameSubstring))
            sql.Append(" AND product_name ILIKE '%' || @nameSubstring || '%'");

        if (minPrice.HasValue)
            sql.Append(" AND product_price >= @minPrice");

        if (maxPrice.HasValue)
            sql.Append(" AND product_price <= @maxPrice");

        sql.Append(" ORDER BY product_id LIMIT @pageSize OFFSET @pageIndex");

        using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync().ConfigureAwait(false);

        // the other way to disable CA2100 is to use just string concatenation
        // but StringBuilder is better in this context (to not create new string every time)
        #pragma warning disable CA2100
        using var command = new NpgsqlCommand(sql.ToString(), connection);
        #pragma warning restore CA2100

        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@pageIndex", pageIndex * pageSize);

        if (!string.IsNullOrEmpty(nameSubstring))
            command.Parameters.AddWithValue("@nameSubstring", nameSubstring);
        if (minPrice.HasValue)
            command.Parameters.AddWithValue("@minPrice", minPrice.Value);
        if (maxPrice.HasValue)
            command.Parameters.AddWithValue("@maxPrice", maxPrice.Value);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var products = new List<Product>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
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