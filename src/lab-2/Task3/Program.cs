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
// OrderRepository orderRepository = scope.ServiceProvider.GetRequiredService<OrderRepository>();
//
// CancellationToken cancellationToken = CancellationToken.None;
//
// // 1. Create a new order
// Console.WriteLine("Creating order...");
// long orderId = await orderRepository.CreateOrder("test_user", cancellationToken).ConfigureAwait(false);
// Console.WriteLine($"Order created with ID: {orderId}");
//
// // 2. Update the order status
// Console.WriteLine("Updating order status...");
// await orderRepository.UpdateOrderStatus(orderId, OrderState.Processing, cancellationToken).ConfigureAwait(false);
// Console.WriteLine("Order status updated to 'processing'.");
//
// // // 3. Search for orders
// Console.WriteLine("Searching for orders...");
// IEnumerable<Order> orders = await orderRepository.SearchOrders(
//     pageIndex: 0,
//     pageSize: 10,
//     cancellationToken: cancellationToken,
//     author: "test_user",
//     state: OrderState.Processing).ConfigureAwait(false);
//
// Console.WriteLine("Orders found:");
// foreach (Order order in orders)
// {
//     Console.WriteLine($"Order ID: {order.Id}, Status: {order.State}, Created At: {order.CreatedAt}, Created By: {order.CreatedBy}");
// }

// //-------------------------------- order item tests --------------------------------//
// OrderItemRepository orderItemRepository = serviceProvider.GetRequiredService<OrderItemRepository>();
// CancellationToken cancellationToken = CancellationToken.None;
//
// // Testing creation of an order item
// Console.WriteLine("Creating an order item...");
// long newOrderItemId = await orderItemRepository.CreateOrderItem(
//     orderId: 10,
//     productId: 1,
//     quantity: 5,
//     cancellationToken: cancellationToken).ConfigureAwait(false);
// Console.WriteLine($"Order item created with ID: {newOrderItemId}");
//
// // Testing soft-deletion of an order item
// Console.WriteLine("Soft deleting the order item...");
// await orderItemRepository.SoftDeleteItem(
//     orderItemId: newOrderItemId,
//     cancellationToken: cancellationToken).ConfigureAwait(false);
// Console.WriteLine("Order item soft-deleted.");
//
// // Testing paginated search for order items
// Console.WriteLine("Searching for order items...");
// IEnumerable<OrderItem> orderItems = await orderItemRepository.SearchOrderItems(
//     pageIndex: 0,
//     pageSize: 10,
//     cancellationToken: cancellationToken,
//     productId: null,
//     isDeleted: true,
//     quantity: null).ConfigureAwait(false);
//
// // Display the search result
// Console.WriteLine("Order items found:");
// foreach (OrderItem item in orderItems)
// {
//     Console.WriteLine($"Order Item ID: {item.Id}, Order ID: {item.OrderId}, Product ID: {item.ProductId}, Quantity: {item.ItemQuantity}, Is Deleted: {item.IsOrderItemDeleted}");
// }

//-------------------------------- order history tests --------------------------------//
OrderHistoryRepository orderHistoryRepository = serviceProvider.GetRequiredService<OrderHistoryRepository>();
CancellationToken cancellationToken = CancellationToken.None;

// Testing creation of an order history item
Console.WriteLine("Creating an order history item...");
long newOrderHistoryItemId = await orderHistoryRepository.CreateOrderHistory(
    orderId: 10,
    orderHistoryItemKind: OrderHistoryItemKind.Created,
    payload: "{\"description\":\"Order created\"}",
    cancellationToken: cancellationToken).ConfigureAwait(false);
Console.WriteLine($"Order history item created with ID: {newOrderHistoryItemId}");

// Testing paginated search for order history items
Console.WriteLine("Searching for order history items...");
IEnumerable<OrderHistoryItem> orderHistoryItems = await orderHistoryRepository.GetOrderHistory(
    pageIndex: 0,
    pageSize: 10,
    cancellationToken: cancellationToken,
    historyItemKind: OrderHistoryItemKind.Created,
    orderId: 10).ConfigureAwait(false);

// Display the search result
Console.WriteLine("Order history items found:");
foreach (OrderHistoryItem item in orderHistoryItems)
{
    Console.WriteLine($"Order History Item ID: {item.Id}, Order ID: {item.OrderId}, Created At: {item.CreatedAt}, Kind: {item.Kind}, Payload: {item.Payload}");
}