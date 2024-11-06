namespace Task3.Dal.Models;

public class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int ItemQuantity { get; set; }

    public bool IsOrderItemDeleted { get; set; }
}