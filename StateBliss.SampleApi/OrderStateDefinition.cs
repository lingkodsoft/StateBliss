using System;
using System.Linq;
using StateBliss.SampleApi.Controllers;

namespace StateBliss.SampleApi
{
    public class OrderStateDefinition : IStateDefinition
    {
        private readonly OrdersRepository _ordersRepository;
//        private readonly OrderStateGuardsForChangingFromInitialToPaid _orderStateGuardsForChangingFromInitialToPaid;

        public OrderStateDefinition(OrdersRepository ordersRepository)
//            ,
//            OrderStateGuardsForChangingFromInitialToPaid orderStateGuardsForChangingFromInitialToPaid)
        {
            _ordersRepository = ordersRepository;
//            _orderStateGuardsForChangingFromInitialToPaid = orderStateGuardsForChangingFromInitialToPaid;
        }
        
        public Type EnumType => typeof(OrderState);



        public State DefineState()
        {
            return new State<Order, OrderState>(a => a.Uid, a => a.State)
                .Define(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
//                        .Changing(_orderStateGuardsForChangingFromInitialToPaid.GetHandler())
//                        .Changed(ChangedHandler5)
//                        .Changed<PayOrderChangeTrigger>(ChangedHandler3)
//                        .Changed<OtherCommand>(ChangedHandler4)
                        
//                        .Changed<PayOrderChangeTrigger, (int id, string data)>(
//                            () => (1, "test1"),
//                            ChangedHandler9)
////                        
                        .Changed<PayOrderChangeTrigger, (int id, string data)>(
                            () => (1, "test2"),
                            ChangedHandler10, 
                            ChangedHandler11)
                    
                        
                        
                        
                        
                        
                        
//                        .Changed(
//                            Handlers.From<OrderState, PaymentCommand>(
//                                () => new PaymentCommand(),
//                            ChangedHandler6,
//                            ChangedHandler7
//                        ))
                        
//                        .Changed(Handlers.From(() => new PaymentCommand(),
//                             ChangedHandler6,
//                             ChangedHandler7
//                            ))
                        
                        ;
//                        .Changed(Handlers.From(() => new PaymentCommand(),
//                            ChangedHandler3, ChangedHandler4));
                        //.Changing<PaymentCommand>(ChangingHandler1);
                        
                    
                    
                    b.From(OrderState.Paid).To(OrderState.Processing);
                    b.From(OrderState.Processing).To(OrderState.Processed);
//                    
//                    b.OnExiting(OrderState.Initial, Handlers.From(() => new PaymentGuardContext(),
//                                OnExitingHandler1, OnExitingHandler2
//                        ));
                    
                });
        }

        private void ChangedHandler11(PayOrderChangeTrigger command, (int id, string data) context)
        {
        }

        private void ChangedHandler10(PayOrderChangeTrigger command, (int id, string data) context)
        {
        }

        private void ChangedHandler9(PayOrderChangeTrigger command, (int id, string data) context)
        {
        }

        private void ChangedHandler4(OtherCommand command)
        {
        }

        private void ChangedHandler3(PayOrderChangeTrigger command)
        {
        }

        private void ChangedHandler5(TriggerCommand<OrderState> command)
        {
        }
    }
}