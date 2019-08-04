using System;
using System.Linq;

namespace StateBliss.SampleApi
{
    public class OrderStateDefinition : IStateDefinition
    {
        private readonly OrdersRepository _ordersRepository;

        public OrderStateDefinition(OrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }
        
        public Type EnumType => typeof(OrderState);
        
        public State DefineState(Guid id)
        {
            var order = _ordersRepository.GetOrders().Single(a => a.Uid == id);
            
            return new State<Order, OrderState>(order, a => a.Uid, a => a.State)
                .Define(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid);
                    b.From(OrderState.Paid).To(OrderState.Processing);
                    b.From(OrderState.Processing).To(OrderState.Processed);
                    
                    b.OnEntering(OrderState.Paid, Guards.From<PaymentGuardContext>(
                        ValidateRequest,
                        PayToPaymentGateway,
                        PersistOrderToRepository
                        ));
                });
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