using Bll.Services;
using Dal.Models.Enums;
using Dal.Serializators;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility.ConcreteHandlers;

public class PackingStartedHandler : HandlerBase
{
    private readonly OrderService _orderService;

    public PackingStartedHandler(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.PackingStarted)
        {
            OrderProcessingValue.Types.OrderPackingStarted packing = message.Value.PackingStarted;

            await _orderService.LogOrderHistory(
                new OrderHistoryData(
                    packing.OrderId,
                    OrderHistoryItemKind.StateChanged,
                    "packing"),
                cancellationToken);
        }
        else
        {
            await base.HandleAsync(message, cancellationToken);
        }
    }
}