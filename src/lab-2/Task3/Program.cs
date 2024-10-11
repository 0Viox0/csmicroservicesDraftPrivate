using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Task3.Dal.Migrations;
using Task3.Dal.Models;
using Task3.Dal.Models.Enums;
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

using IServiceScope scope = serviceProvider.CreateScope();
IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

//-------------------------------- product tests --------------------------------//
// ProductRepository productRepository = serviceProvider.GetRequiredService<ProductRepository>();
//
// CancellationToken cancellationToken = CancellationToken.None;
//
// // Testing creation of the product
// Console.WriteLine("Creating product...");
// await productRepository.CreateProduct("Sample Product", 19.99m, cancellationToken).ConfigureAwait(false);
// Console.WriteLine("Product created.");
//
// // Testing searching of the product
// Console.WriteLine("Searching for products...");
// IEnumerable<Product> products = await productRepository.SearchProduct(
//     pageIndex: 0,
//     pageSize: 10,
//     nameSubstring: "Sample",
//     maxPrice: null,
//     minPrice: null,
//     cancellationToken: cancellationToken).ConfigureAwait(false);
//
// // Display the result
// Console.WriteLine("Products found:");
// foreach (Product product in products)
// {
//     Console.WriteLine($"Product ID: {product.Id}, Name: {product.Name}, Price: {product.Price}");
// }

//-------------------------------- orders tests --------------------------------//
OrderRepository orderRepository = scope.ServiceProvider.GetRequiredService<OrderRepository>();

CancellationToken cancellationToken = CancellationToken.None;

// 1. Create a new order
Console.WriteLine("Creating order...");
long orderId = await orderRepository.CreateOrder("test_user", cancellationToken).ConfigureAwait(false);
Console.WriteLine($"Order created with ID: {orderId}");

// 2. Update the order status
Console.WriteLine("Updating order status...");
await orderRepository.UpdateOrderStatus(orderId, OrderState.Processing, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Order status updated to 'processing'.");

// // 3. Search for orders
Console.WriteLine("Searching for orders...");
IEnumerable<Order> orders = await orderRepository.SearchOrders(
    pageIndex: 0,
    pageSize: 10,
    cancellationToken: cancellationToken,
    author: "test_user",
    state: OrderState.Processing).ConfigureAwait(false);

Console.WriteLine("Orders found:");
foreach (Order order in orders)
{
    Console.WriteLine($"Order ID: {order.OrderId}, Status: {order.OrderState}, Created At: {order.CreatedAt}, Created By: {order.CreatedBy}");
}