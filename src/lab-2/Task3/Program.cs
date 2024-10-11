using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Task3.Dal.Migrations;
using Task3.Dal.Models;
using Task3.Dal.Repositories;
using Task3.Dal.RepositoryExtensions;

const string connectionString = "Host=localhost;Username=viox;Password=123;Database=hehe";

var datasource = NpgsqlDataSource.Create(connectionString);

ServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton(datasource)
    .AddRepositories()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CreateProductTable).Assembly).For.Migrations())

    // .AddLogging(lb => lb.AddFluentMigratorConsole())
    .BuildServiceProvider();

// Perform migration
using IServiceScope scope = serviceProvider.CreateScope();
IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

// Retrieve the ProductRepository from the DI container
ProductRepository productRepository = serviceProvider.GetRequiredService<ProductRepository>();

// Define a cancellation token
CancellationToken cancellationToken = CancellationToken.None;

// 1. Create a new product
Console.WriteLine("Creating product...");
await productRepository.CreateProduct("Sample Product", 19.99m, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Product created.");

// 2. Search for products
Console.WriteLine("Searching for products...");
IEnumerable<Product> products = await productRepository.SearchProduct(
    pageIndex: 0,
    pageSize: 10,
    nameSubstring: "Sample",
    maxPrice: null,
    minPrice: null,
    cancellationToken: cancellationToken).ConfigureAwait(false);

// Display the result
Console.WriteLine("Products found:");
foreach (Product product in products)
{
    Console.WriteLine($"Product ID: {product.Id}, Name: {product.Name}, Price: {product.Price}");
}