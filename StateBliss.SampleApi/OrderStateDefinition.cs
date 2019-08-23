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



        private class TestTarget
        {
            public void ChangeHandler21(PayOrderChangeTrigger command)
            {
            }

            public void ChangeHandler20(TriggerCommand<OrderState> command)
            {
            }

            public void ChangeHandler22(PayOrderChangeTrigger command, (int id, string localData) context)
            {
            }

            public void ChangeHandler23(PayOrderChangeTrigger command, (int id, string localData) context)
            {
            }

            public void ChangingHandler20(TriggerCommand<OrderState> command, GuardStateContext<OrderState> context)
            {
                context.Continue = true;
            }

            public void ChangingHandler21(PayOrderChangeTrigger command, GuardStateContext<OrderState> context)
            {
                context.Continue = true;
            }

            public void ChangingHandler22(PayOrderChangeTrigger command, TestGuardContext context)
            {
                context.Continue = true;
            }

            public void ChangingHandler23(PayOrderChangeTrigger command, TestGuardContext context)
            {
                context.Continue = true;
            }
        }


        private class TestGuardContext : GuardStateContext<OrderState>
        {
            
        }
        
        public State DefineState()
        {
            return new State<Order, OrderState>(a => a.Uid, a => a.State)
                .Define(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
//                        .Changing(_orderStateGuardsForChangingFromInitialToPaid.GetHandler())
                        .Changed(ChangedHandler5)
                        .Changed<PayOrderChangeTrigger>(ChangedHandler3)
                        .Changed<OtherCommand>(ChangedHandler4)

                        .Changed<PayOrderChangeTrigger, (int id, string data)>(
                            () => (1, "test1"),
                            ChangedHandler9)

                        .Changed<PayOrderChangeTrigger, (int id, string data)>(
                            () => (1, "test2"),
                            ChangedHandler10,
                            ChangedHandler11)

                        .Changed(new TestTarget(), t => t.ChangeHandler20)

                        .Changed<PayOrderChangeTrigger, TestTarget>(new TestTarget(), t => t.ChangeHandler21)

                        .Changed<PayOrderChangeTrigger, TestTarget, (int id, string localData)>(
                            new TestTarget(), () => (2, "test3"), t => t.ChangeHandler22, t => t.ChangeHandler23)

                        .Changing(ChangingHandler5)
                        .Changing<PayOrderChangeTrigger>(ChangingHandler3)
                        .Changing<OtherCommand>(ChangingHandler4)

                        .Changing<PayOrderChangeTrigger>(
                            ChangingHandler25,
                            ChangingHandler26)

                        .Changing<PayOrderChangeTrigger>(ChangingHandler9)
                        
                        .Changing<PayOrderChangeTrigger, TestGuardContext>(
                            () => new TestGuardContext(),
                            ChangingHandler9)

                        .Changing<PayOrderChangeTrigger, TestGuardContext>(
                            () => new TestGuardContext(),
                            ChangingHandler10,
                            ChangingHandler11)

                        .Changing(new TestTarget(), t => t.ChangingHandler20)

                        .Changing<PayOrderChangeTrigger, TestTarget>(new TestTarget(), t => t.ChangingHandler21)

                        .Changing<PayOrderChangeTrigger, TestTarget, TestGuardContext>(
                            new TestTarget(), () => new TestGuardContext(),
                            t => t.ChangingHandler22, t => t.ChangingHandler23);

                        
                    
                    b.From(OrderState.Paid).To(OrderState.Processing);
                    b.From(OrderState.Processing).To(OrderState.Processed);

                    b.OnEntered(OrderState.Paid, OnEnteredHandler1);
                    
                    b.OnEntered<PayOrderChangeTrigger>(OrderState.Paid, OnEnteredHandler2, OnEnteredHandler20);
                    
                    b.OnEntered<PayOrderChangeTrigger, TestGuardContext>(OrderState.Paid,
                        () => new TestGuardContext(),
                        OnEnteredHandler3,
                        OnEnteredHandler4);
                });
        }

        private void OnEnteredHandler4(PayOrderChangeTrigger command, TestGuardContext context)
        {
             
        }        

        private void OnEnteredHandler3(PayOrderChangeTrigger command, TestGuardContext context)
        {
    
        }

        private void OnEnteredHandler20(PayOrderChangeTrigger command, StateContext<OrderState> context)
        {

        }

        private void OnEnteredHandler2(PayOrderChangeTrigger command, StateContext<OrderState> context)
        {
            
        }

        private void OnEnteredHandler1(TriggerCommand<OrderState> command)
        {
            
        }

        private void ChangingHandler26(PayOrderChangeTrigger command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
        }

        private void ChangingHandler25(PayOrderChangeTrigger command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
        }

        private void ChangingHandler11(PayOrderChangeTrigger command, TestGuardContext context)
        {
            context.Continue = true;
        }

        private void ChangingHandler10(PayOrderChangeTrigger command, TestGuardContext context)
        {
            context.Continue = true;
        }

        private void ChangingHandler9(PayOrderChangeTrigger command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
        }

        private void ChangingHandler4(OtherCommand command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
        }

        private void ChangingHandler3(PayOrderChangeTrigger command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
        }

        private void ChangingHandler5(TriggerCommand<OrderState> command, GuardStateContext<OrderState> context)
        {
            context.Continue = true;
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