using Task3.Dal.Models.Enums;

namespace Task3.Dal.Models;

 public class Order
{
    public long OrderId { get; set; }

    public OrderState? OrderState { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
}
