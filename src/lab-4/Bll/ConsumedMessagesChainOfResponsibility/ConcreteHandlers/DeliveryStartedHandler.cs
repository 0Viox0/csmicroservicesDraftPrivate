using Bll.Services;
using Dal.Models.Enums;
using Dal.Serializators;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility.ConcreteHandlers;

public class DeliveryStartedHandler : HandlerBase
{
    private readonly OrderService _orderService;

    public DeliveryStartedHandler(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.DeliveryStarted)
        {
            OrderProcessingValue.Types.OrderDeliveryStarted delivery = message.Value.DeliveryStarted;

            await _orderService.LogOrderHistory(
                new OrderHistoryData(
                    delivery.OrderId,
                    OrderHistoryItemKind.StateChanged,
                    "delivery started"),
                cancellationToken);
        }
        else
        {
            await base.HandleAsync(message, cancellationToken);
        }
    }
}