using System;
using System.Linq;

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
        
        public State DefineState(Guid id)
        {
            var order = _ordersRepository.GetOrders().Single(a => a.Uid == id);
            
            return new State<Order, OrderState>(order, a => a.Uid, a => a.State)
                .Define(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
                        .Changing(_orderStateGuardsForChangingFromInitialToPaid.GetHandler());
                    
                    b.From(OrderState.Paid).To(OrderState.Processing);
                    b.From(OrderState.Processing).To(OrderState.Processed);
                    
                });
        }
    }
}