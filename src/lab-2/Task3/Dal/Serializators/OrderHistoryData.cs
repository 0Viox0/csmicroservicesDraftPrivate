using Task3.Dal.Models.Enums;

namespace Task3.Dal.Serializators;

public record OrderHistoryData(long OrderId, OrderHistoryItemKind Kind, string Message);