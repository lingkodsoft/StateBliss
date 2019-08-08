namespace StateBliss.SampleApi
{
    public class OrderStateGuardsForChangingFromInitialToPaid : IStateDefinitionHandler<PaymentGuardContext>
    {
        private readonly OrdersRepository _ordersRepository;

        public OrderStateGuardsForChangingFromInitialToPaid(OrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }
        
        public IHandlersInfo<PaymentGuardContext> GetHandler()
        {
            return Handlers.From(() =>
                {
                    var context = new PaymentGuardContext();
                    context.Data["test"] = "test";
                    return context;
                },
                ValidateRequest,
                PayToPaymentGateway,
                PersistOrderToRepository);
        }
        
        private void ValidateRequest(PaymentGuardContext context)
        {
            context.ValidateRequest_CallCount++;
            context.Continue = true;
        }

        private void PayToPaymentGateway(PaymentGuardContext context)
        {
            context.PayToGateway_CallCount++;
            context.Continue = true;
        }

        private void PersistOrderToRepository(PaymentGuardContext context)
        {
            var order = context.ParentContext.Order;
            context.PersistToRepo_CallCount++;
            _ordersRepository.UpdateOrder(order);

            if (context.Data["test"] == "test")
            {
                context.Continue = true;   
            }
        }
    }
}