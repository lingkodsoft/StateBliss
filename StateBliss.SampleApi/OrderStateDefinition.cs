using System;
using System.Linq;
using StateBliss.SampleApi.Controllers;

namespace StateBliss.SampleApi
{
    public class OrderStateDefinition : IStateDefinition
    {
        private readonly OrdersRepository _ordersRepository;
        private readonly OrderStateGuardsForChangingFromInitialToPaid _orderStateGuardsForChangingFromInitialToPaid;

        public OrderStateDefinition(OrdersRepository ordersRepository,
            OrderStateGuardsForChangingFromInitialToPaid orderStateGuardsForChangingFromInitialToPaid)
        {
            _ordersRepository = ordersRepository;
            _orderStateGuardsForChangingFromInitialToPaid = orderStateGuardsForChangingFromInitialToPaid;
        }
        
        public Type EnumType => typeof(OrderState);
        
        public State DefineState()
        {
            return new State<Order, OrderState>(a => a.Uid, a => a.State)
                .Define(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
                        .Changing(_orderStateGuardsForChangingFromInitialToPaid.GetHandler())
                        .Changed(Handlers.From(() => new PaymentHandlerContext(),
                            ChangedHandler1, ChangedHandler2
                            
                            ));
                    
                    b.From(OrderState.Paid).To(OrderState.Processing);
                    b.From(OrderState.Processing).To(OrderState.Processed);
                    
                });
        }

        private void ChangedHandler2(PaymentHandlerContext context)
        {
           // throw new NotImplementedException();
        }

        private void ChangedHandler1(PaymentHandlerContext context)
        {
            
            // throw new NotImplementedException();
        }
    }
}