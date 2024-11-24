using Dal.Models.Enums;

namespace Dal.Serializators;

public record OrderHistoryData(long OrderId, OrderHistoryItemKind Kind, string Message);