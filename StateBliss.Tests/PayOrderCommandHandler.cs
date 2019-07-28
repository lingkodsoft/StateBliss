namespace StateBliss.Tests
{
    public class PayOrderCommandHandler
    {
        private readonly OrderStateDefinition _orderStateDefinition;
        private readonly OrdersRepository _ordersRepository;

        public PayOrderCommandHandler(OrderStateDefinition orderStateDefinition, OrdersRepository ordersRepository)
        {
            _orderStateDefinition = orderStateDefinition;
            _ordersRepository = ordersRepository;
        }

        private class Context : GuardContext<OrderState>
        {
            public PayOrderCommand Command { get; set; }
        }
        
        public void Handle(PayOrderCommand cmd)
        {
            var state = _orderStateDefinition.GetState(cmd.Order);
            var context = new Context
            {
                Command = cmd
            };
            
            var hasChangedState = state.ChangeTo(OrderState.Paid, 
                Guards<OrderState>.From(context,
                    ValidateRequest,
                    PayToPaymentGateway, 
                    PersistOrderToRepository
            ));

            cmd.State = state;
            cmd.Succeeded = hasChangedState;
        }

        private void ValidateRequest(Context context)
        {
            context.Continue = true;
        }

        private void PayToPaymentGateway(Context context)
        {
            context.Continue = true;
        }

        private void PersistOrderToRepository(Context context)
        {
            _ordersRepository.UpdateOrder(context.Command.Order);
            context.Continue = true;
        }
    }
}