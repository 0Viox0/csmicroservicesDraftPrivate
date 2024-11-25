using Bll.Services;
using Dal.Models.Enums;
using Dal.Serializators;
using Kafka.Models;
using Orders.Kafka.Contracts;

namespace Bll.ConsumedMessagesChainOfResponsibility.ConcreteHandlers;

public class ApprovalReceivedHandler : HandlerBase
{
    private readonly OrderService _orderService;

    public ApprovalReceivedHandler(OrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task HandleAsync(
        KafkaMessage<OrderProcessingKey, OrderProcessingValue> message,
        CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.ApprovalReceived)
        {
            OrderProcessingValue.Types.OrderApprovalReceived approval = message.Value.ApprovalReceived;

            string orderHistoryMessage;

            if (!approval.IsApproved)
            {
                await _orderService.CancelOrder(approval.OrderId, cancellationToken);
                orderHistoryMessage = "order was canceled";
            }
            else
            {
                orderHistoryMessage = "order approved";
            }

            await _orderService.LogOrderHistory(
                new OrderHistoryData(
                    approval.OrderId,
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