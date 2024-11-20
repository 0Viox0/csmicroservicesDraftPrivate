namespace GrpcClientHttpGateway.Models;

public record OrderHistoryItemReturnModel(
    long Id,
    long OrderId,
    string? CreatedAt,
    OrderHistoryItemKind OrderHistoryItemKind,
    OrderHistoryPayloadBase? Payload);