namespace Task3.Bll.Dtos.OrderDtos;

public class OrderHistoryItemSearchDto
{
    public long OrderId { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}