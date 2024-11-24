namespace Dal.Models;

public class OrderHistoryItemSearchDto
{
    public long OrderId { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}