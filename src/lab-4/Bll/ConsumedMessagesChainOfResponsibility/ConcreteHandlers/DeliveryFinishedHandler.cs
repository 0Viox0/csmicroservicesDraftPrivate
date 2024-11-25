using Bll.Services;
using Dal.Models.Enums;
using Dal.Serializators;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility.ConcreteHandlers;

public class DeliveryFinishedHandler : HandlerBase
{
    private readonly OrderService _orderService;

    public DeliveryFinishedHandler(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.DeliveryFinished)
        {
            OrderProcessingValue.Types.OrderDeliveryFinished delivery = message.Value.DeliveryFinished;

            string orderHistoryMessage;

            if (!delivery.IsFinishedSuccessfully)
            {
                await _orderService.CancelOrder(delivery.OrderId, cancellationToken);
                orderHistoryMessage = $"Message: {delivery.FailureReason}. Order was canceled";
            }
            else
            {
                orderHistoryMessage = "order delivered";
            }

            await _orderService.LogOrderHistory(
                new OrderHistoryData(
                    delivery.OrderId,
                    OrderHistoryItemKind.StateChanged,
                    orderHistoryMessage),
                cancellationToken);

            if (delivery.IsFinishedSuccessfully)
                await _orderService.FulfillOrder(delivery.OrderId, cancellationToken);
        }
        else
        {
            await base.HandleAsync(message, cancellationToken);
        }
    }
}