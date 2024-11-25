namespace GrpcClientHttpGateway.Models.PayloadModels;

public record ItemAddedPayload(long ProductId, int Quantity) : OrderHistoryPayloadBase;
