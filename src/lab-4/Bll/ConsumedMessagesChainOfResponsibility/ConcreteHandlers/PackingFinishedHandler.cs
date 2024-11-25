using Bll.Services;
using Dal.Models.Enums;
using Dal.Serializators;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility.ConcreteHandlers;

public class PackingFinishedHandler : HandlerBase
{
    private readonly OrderService _orderService;

    public PackingFinishedHandler(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.PackingFinished)
        {
            OrderProcessingValue.Types.OrderPackingFinished packing = message.Value.PackingFinished;

            string orderHistoryMessage;

            if (!packing.IsFinishedSuccessfully)
            {
                await _orderService.CancelOrder(packing.OrderId, cancellationToken);
                orderHistoryMessage = $"order was canceled, reason: {packing.FailureReason}";
            }
            else
            {
                orderHistoryMessage = "order packed";
            }

            await _orderService.LogOrderHistory(
                new OrderHistoryData(
                    packing.OrderId,
                    OrderHistoryItemKind.StateChanged,
                    orderHistoryMessage),
                cancellationToken);
        }
        else
        {
            await base.HandleAsync(message, cancellationToken);
        }
    }
}