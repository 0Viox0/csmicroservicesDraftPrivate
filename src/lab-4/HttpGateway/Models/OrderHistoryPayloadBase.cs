using GrpcClientHttpGateway.Models.PayloadModels;
using System.Text.Json.Serialization;

namespace GrpcClientHttpGateway.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CreatedByPayload), "CreatedBy")]
[JsonDerivedType(typeof(ItemAddedPayload), "ItemAdded")]
[JsonDerivedType(typeof(ItemRemovedPayload), "ItemRemoved")]
[JsonDerivedType(typeof(StateChangedPayload), "StateChanged")]
public abstract record OrderHistoryPayloadBase;