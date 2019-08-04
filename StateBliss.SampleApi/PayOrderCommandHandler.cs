namespace StateBliss.SampleApi
{
    public class PayOrderCommandHandler
    {
        private readonly IStateMachineManager _stateMachineManager;
        private readonly OrdersRepository _ordersRepository;

        public PayOrderCommandHandler(IStateMachineManager stateMachineManager, OrdersRepository ordersRepository)
        {
            _stateMachineManager = stateMachineManager;
            _ordersRepository = ordersRepository;
        }
            
        public void Handle(PayOrderCommand cmd)
        {
            var context = new PaymentGuardContext
            {
                Command = cmd
            };

            _ordersRepository.InsertOrder(cmd.Order);
            
            var state = _stateMachineManager.GetState<OrderState>(cmd.Order.Uid);
            
            state.GuardsForEntry(OrderState.Paid, Guards.From(context, 
                ValidateRequest, PayToPaymentGateway, PersistOrderToRepository
                ));
            
            var hasChangedState = state.ChangeTo(OrderState.Paid, context);
            
            cmd.State = (State<Order, OrderState>)state;
            cmd.Succeeded = hasChangedState;
        }

        private void ValidateRequest(PaymentGuardContext context)
        {
            context.Command.ValidateRequest_CallCount++;
            context.Continue = true;
        }

        private void PayToPaymentGateway(PaymentGuardContext context)
        {
            context.Command.PayToGateway_CallCount++;
            context.Continue = true;
        }

        private void PersistOrderToRepository(PaymentGuardContext context)
        {
            context.Command.PersistToRepo_CallCount++;
            _ordersRepository.UpdateOrder(context.Command.Order);
            context.Continue = true;
        }
            
    }
}