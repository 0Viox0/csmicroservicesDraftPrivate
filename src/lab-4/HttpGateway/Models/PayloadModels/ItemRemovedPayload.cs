namespace GrpcClientHttpGateway.Models.PayloadModels;

public record ItemRemovedPayload(long ProductId) : OrderHistoryPayloadBase;