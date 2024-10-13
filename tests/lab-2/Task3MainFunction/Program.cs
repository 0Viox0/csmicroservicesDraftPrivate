using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task2.ConfigurationProviders;
using Task2.Extensions;
using Task2.Service;
using Task3.Bll.Dtos.OrderDtos;
using Task3.Bll.Dtos.ProductDtos;
using Task3.Bll.Extensions;
using Task3.Bll.Services;
using Task3.Dal.RepositoryExtensions;

namespace Task3MainFunction;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configurationManger = new ConfigurationManager();
        IConfigurationBuilder configurationBuilder = configurationManger;

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddConfigurationManagerBasePath(configurationManger)
            .AddExternalServiceOptions(configurationManger)
            .AddDatabaseOptions(configurationManger)
            .AddCustomConfiguration(TimeSpan.FromSeconds(2))
            .AddOrderHistoryDataJsonSerializer()
            .AddRepositories()
            .AddBllServices()
            .AddMigrations()
            .AddNpgsqlDataSource()
            .BuildServiceProvider();

        using IServiceScope scope = serviceProvider.CreateScope();

        // Add our custom configuration source
        configurationBuilder
            .Add(scope.ServiceProvider.GetRequiredService<CustomConfigurationProviderSource>());

        // Update configuration using ConfigurationUpdateService
        ConfigurationUpdateService configurationUpdateService =
            scope.ServiceProvider.GetRequiredService<ConfigurationUpdateService>();
        await configurationUpdateService.UpdateConfiguration(10, string.Empty, CancellationToken.None).ConfigureAwait(false);

        // Run migrations
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

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
    }
}
