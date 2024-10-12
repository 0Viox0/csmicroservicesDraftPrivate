#pragma warning disable

using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Task2.ConfigurationProviders;
using Task2.Extensions;
using Task2.Service;
using Task3.Bll;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Dtos.ProductDtos;
using Task3.Bll.Extensions;
using Task3.Bll.Services;
using Task3.Dal.Migrations;
using Task3.Dal.RepositoryExtensions;

var configurationManger = new ConfigurationManager();
IConfigurationBuilder configurationBuilder = configurationManger;

ServiceProvider serviceProvider = new ServiceCollection()
    .AddCustomConfiguration("http://localhost:8080", TimeSpan.FromSeconds(2))
    .AddDatabaseConfiguration(configurationManger)
    .AddRepositories()
    .AddBllServices()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value.ConnectionString)
        .ScanIn(typeof(CreateProductTable).Assembly).For.Migrations())

    // .AddLogging(lb => lb.AddFluentMigratorConsole())
    .AddSingleton(sp =>
    {
        DatabaseSettings dbSettings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        return NpgsqlDataSource.Create(dbSettings.ConnectionString);
    })
    .BuildServiceProvider();

using IServiceScope scope = serviceProvider.CreateScope();

configurationBuilder.Add(scope.ServiceProvider.GetRequiredService<CustomConfigurationProviderSource>());

ConfigurationUpdateService configurationUpdateService =
    scope.ServiceProvider.GetRequiredService<ConfigurationUpdateService>();
await configurationUpdateService.UpdateConfiguration(10, string.Empty, CancellationToken.None).ConfigureAwait(false);

// Run migrations
IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

// const string connectionString = "Host=localhost;Port=5433;Username=viox;Password=123;Database=hehe";
// string connectionString = databaseSettings.Value.ConnectionString;

// var datasource = NpgsqlDataSource.Create(connectionString);
//
// ServiceProvider serviceProvider = new ServiceCollection()
//     .AddSingleton(datasource)
//     .AddRepositories()
//     .AddBllServices()
//     .AddFluentMigratorCore()
//     .ConfigureRunner(rb => rb
//         .AddPostgres()
//         .WithGlobalConnectionString(connectionString)
//         .ScanIn(typeof(CreateProductTable).Assembly).For.Migrations())
//
//     // .AddLogging(lb => lb.AddFluentMigratorConsole())
//     .BuildServiceProvider();
//
// using IServiceScope scope = serviceProvider.CreateScope();
// IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
// runner.MigrateUp();
//
//-------------------------------- main functionality tests --------------------------------//
ProductService productService = scope.ServiceProvider.GetRequiredService<ProductService>();
OrderService orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

CancellationToken cancellationToken = CancellationToken.None;

// Step 1: Create several products
var product1 = new ProductCreationDto { Name = "Product 1", Price = 100.0m };
var product2 = new ProductCreationDto { Name = "Product 2", Price = 150.0m };
var product3 = new ProductCreationDto { Name = "Product 3", Price = 200.0m };

await productService.CreateProduct(product1, cancellationToken).ConfigureAwait(false);
await productService.CreateProduct(product2, cancellationToken).ConfigureAwait(false);
await productService.CreateProduct(product3, cancellationToken).ConfigureAwait(false);

Console.WriteLine("Products were created.");

// Step 2: Create an order
string createdBy = "User1";
long orderId = await orderService.CreateOrder(createdBy, cancellationToken).ConfigureAwait(false);
Console.WriteLine($"Order {orderId} was created by {createdBy}.");

// Step 3: Add products to the order
var orderItem1 = new OrderItemCreationDto { OrderId = orderId, ProductId = 1, Quantity = 2 };
var orderItem2 = new OrderItemCreationDto { OrderId = orderId, ProductId = 2, Quantity = 1 };
var orderItem3 = new OrderItemCreationDto { OrderId = orderId, ProductId = 3, Quantity = 5 };

await orderService.AddProductToOrder(orderItem1, cancellationToken).ConfigureAwait(false);
await orderService.AddProductToOrder(orderItem2, cancellationToken).ConfigureAwait(false);
await orderService.AddProductToOrder(orderItem3, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Products were added to the order.");

// Step 4: Remove one product from the order
var removeItemDto = new OrderItemRemoveDto { OrderId = orderId, ProductId = 2 };
await orderService.RemoveProductFromOrder(removeItemDto, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Product with id 2 was removed from the order.");

// Step 5: Transfer the order to processing
await orderService.TransferOrderToProcessing(orderId, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Order was transferred to processing.");

// Step 6: Fulfill the order
await orderService.FulfillOrder(orderId, cancellationToken).ConfigureAwait(false);
Console.WriteLine("Order was fulfilled.");

// Step 7: Output the entire order history to the console
var orderHistorySearchDto = new OrderHistoryItemSearchDto { OrderId = orderId, PageIndex = 0, PageSize = 10 };
IEnumerable<OrderHistoryReturnItemDto> orderHistory =
    await orderService.GetOrderHistory(orderHistorySearchDto, cancellationToken).ConfigureAwait(false);

Console.WriteLine("Displaying order history:");
foreach (OrderHistoryReturnItemDto historyItem in orderHistory)
{
    Console.WriteLine($"{historyItem.CreatedAt}: {historyItem.Kind} - {historyItem.Payload}");
}

// //-------------------------------- product repository tests --------------------------------//
// // ProductRepository productRepository = serviceProvider.GetRequiredService<ProductRepository>();
// //
// // CancellationToken cancellationToken = CancellationToken.None;
// //
// // // Testing creation of the product
// // Console.WriteLine("Creating product...");
// // await productRepository.CreateProduct("Sample Product", 19.99m, cancellationToken).ConfigureAwait(false);
// // Console.WriteLine("Product created.");
// //
// // // Testing searching of the product
// // Console.WriteLine("Searching for products...");
// // IEnumerable<Product> products = await productRepository.SearchProduct(
// //     pageIndex: 0,
// //     pageSize: 10,
// //     nameSubstring: "Sample",
// //     maxPrice: null,
// //     minPrice: null,
// //     cancellationToken: cancellationToken).ConfigureAwait(false);
// //
// // // Display the result
// // Console.WriteLine("Products found:");
// // foreach (Product product in products)
// // {
// //     Console.WriteLine($"Product ID: {product.Id}, Name: {product.Name}, Price: {product.Price}");
// // }
//
// //-------------------------------- orders repository tests --------------------------------//
// // OrderRepository orderRepository = scope.ServiceProvider.GetRequiredService<OrderRepository>();
// //
// // CancellationToken cancellationToken = CancellationToken.None;
// //
// // // 1. Create a new order
// // Console.WriteLine("Creating order...");
// // long orderId = await orderRepository.CreateOrder("test_user", cancellationToken).ConfigureAwait(false);
// // Console.WriteLine($"Order created with ID: {orderId}");
// //
// // // 2. Update the order status
// // Console.WriteLine("Updating order status...");
// // await orderRepository.UpdateOrderStatus(orderId, OrderState.Processing, cancellationToken).ConfigureAwait(false);
// // Console.WriteLine("Order status updated to 'processing'.");
// //
// // // // 3. Search for orders
// // Console.WriteLine("Searching for orders...");
// // IEnumerable<Order> orders = await orderRepository.SearchOrders(
// //     pageIndex: 0,
// //     pageSize: 10,
// //     cancellationToken: cancellationToken,
// //     author: "test_user",
// //     state: OrderState.Processing).ConfigureAwait(false);
// //
// // Console.WriteLine("Orders found:");
// // foreach (Order order in orders)
// // {
// //     Console.WriteLine($"Order ID: {order.Id}, Status: {order.State}, Created At: {order.CreatedAt}, Created By: {order.CreatedBy}");
// // }
//
// // //-------------------------------- order item repository tests --------------------------------//
// // OrderItemRepository orderItemRepository = serviceProvider.GetRequiredService<OrderItemRepository>();
// // CancellationToken cancellationToken = CancellationToken.None;
// //
// // // Testing creation of an order item
// // Console.WriteLine("Creating an order item...");
// // long newOrderItemId = await orderItemRepository.CreateOrderItem(
// //     orderId: 10,
// //     productId: 1,
// //     quantity: 5,
// //     cancellationToken: cancellationToken).ConfigureAwait(false);
// // Console.WriteLine($"Order item created with ID: {newOrderItemId}");
// //
// // // Testing soft-deletion of an order item
// // Console.WriteLine("Soft deleting the order item...");
// // await orderItemRepository.SoftDeleteItem(
// //     orderItemId: newOrderItemId,
// //     cancellationToken: cancellationToken).ConfigureAwait(false);
// // Console.WriteLine("Order item soft-deleted.");
// //
// // // Testing paginated search for order items
// // Console.WriteLine("Searching for order items...");
// // IEnumerable<OrderItem> orderItems = await orderItemRepository.SearchOrderItems(
// //     pageIndex: 0,
// //     pageSize: 10,
// //     cancellationToken: cancellationToken,
// //     productId: null,
// //     isDeleted: true,
// //     quantity: null).ConfigureAwait(false);
// //
// // // Display the search result
// // Console.WriteLine("Order items found:");
// // foreach (OrderItem item in orderItems)
// // {
// //     Console.WriteLine($"Order Item ID: {item.Id}, Order ID: {item.OrderId}, Product ID: {item.ProductId}, Quantity: {item.ItemQuantity}, Is Deleted: {item.IsOrderItemDeleted}");
// // }
//
// // //-------------------------------- order history repository tests --------------------------------//
// // OrderHistoryRepository orderHistoryRepository = serviceProvider.GetRequiredService<OrderHistoryRepository>();
// // CancellationToken cancellationToken = CancellationToken.None;
// //
// // // Testing creation of an order history item
// // Console.WriteLine("Creating an order history item...");
// // long newOrderHistoryItemId = await orderHistoryRepository.CreateOrderHistory(
// //     orderId: 10,
// //     orderHistoryItemKind: OrderHistoryItemKind.Created,
// //     payload: "{\"description\":\"Order created\"}",
// //     cancellationToken: cancellationToken).ConfigureAwait(false);
// // Console.WriteLine($"Order history item created with ID: {newOrderHistoryItemId}");
// //
// // // Testing paginated search for order history items
// // Console.WriteLine("Searching for order history items...");
// // IEnumerable<OrderHistoryItem> orderHistoryItems = await orderHistoryRepository.GetOrderHistory(
// //     pageIndex: 0,
// //     pageSize: 10,
// //     cancellationToken: cancellationToken,
// //     historyItemKind: OrderHistoryItemKind.Created,
// //     orderId: 10).ConfigureAwait(false);
// //
// // // Display the search result
// // Console.WriteLine("Order history items found:");
// // foreach (OrderHistoryItem item in orderHistoryItems)
// // {
// //     Console.WriteLine($"Order History Item ID: {item.Id}, Order ID: {item.OrderId}, Created At: {item.CreatedAt}, Kind: {item.Kind}, Payload: {item.Payload}");
// // }