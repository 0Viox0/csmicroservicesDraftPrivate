namespace GrpcClientHttpGateway.Models.PayloadModels;

public record StateChangedPayload(string NewState) : OrderHistoryPayloadBase;
