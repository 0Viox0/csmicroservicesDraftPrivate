namespace Dal.Repositories;

public class OrderItemCreationDto
{
    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int Quantity { get; set; }
}