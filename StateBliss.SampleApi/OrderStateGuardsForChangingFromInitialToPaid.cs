namespace StateBliss.SampleApi
{
    public class OrderStateGuardsForChangingFromInitialToPaid : IStateDefinitionHandler<PaymentHandlerContext>
    {
        private readonly OrdersRepository _ordersRepository;

        public OrderStateGuardsForChangingFromInitialToPaid(OrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }
        
        public IHandlersInfo<PaymentHandlerContext> GetHandler()
        {
            return Handlers.From(() =>
                {
                    var context = new PaymentHandlerContext();
                    context.Data["test"] = "test";
                    return context;
                },
                ValidateRequest,
                PayToPaymentGateway,
                PersistOrderToRepository);
        }
        
        private void ValidateRequest(PaymentHandlerContext context)
        {
            context.ValidateRequest_CallCount++;
            context.Continue = true;
        }

        private void PayToPaymentGateway(PaymentHandlerContext context)
        {
            context.PayToGateway_CallCount++;
            context.Continue = true;
        }

        private void PersistOrderToRepository(PaymentHandlerContext context)
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