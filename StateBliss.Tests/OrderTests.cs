using System;
using Xunit;

namespace StateBliss.Tests
{
    public class OrderTests
    {
        public class Order
        {
            public int Id { get; set; }
            public OrderState State { get; set; }
            public Guid Uid { get; set; }
        }

        public enum OrderState
        {
            Initial,
            Paid,
            Processing,
            Processed,
            Delivered
        }

        public class OrderHandler
        {
            private class PaymentGuardContext : StateContext<OrderState>
            {
            }

            public void HandleOrder(Order order)
            {
                var context = new PaymentGuardContext();

                var state = new State<Order, OrderState>(a => a.Uid, a => a.State)
                    .Define(b =>
                    {
                        b.From(OrderState.Initial).To(OrderState.Paid)
                            .Changing(OnTransitioningStateFromInitialToPaidHandler)
                            .Changed(OnTransitionedStateFromInitialToPaidHandler);

                        b.From(OrderState.Paid).To(OrderState.Processed);
                        b.From(OrderState.Processed).To(OrderState.Delivered);

                        b.OnEntering(OrderState.Paid, Guards.From(context,
                            ValidateRequest,
                            PayToPaymentGateway,
                            PersistOrderToRepository
                        ));

                        b.DisableSameStateTransitionFor(OrderState.Paid);

                    });

                var hasChangedState = state.ChangeTo(OrderState.Paid);

//        Assert.Equal(OrderState.Paid, order.State);
            }

            public void HandleOrderUseExtensionAndTriggers(Order order)
            {
                var context = new PaymentGuardContext();
                var triggerToOrderStatePaid = "triggerToOrderStatePaid";

                var state = order.State.AsState(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
                        .Changing(OnTransitioningStateFromInitialToPaidHandler);

                    b.TriggerTo(OrderState.Paid, triggerToOrderStatePaid);
                });

                StateMachineManager.Trigger(triggerToOrderStatePaid);

                Assert.Equal(OrderState.Paid, order.State);
            }

            public void HandleOrderUserTrigger(Order order)
            {
                var context = new PaymentGuardContext();
                var triggerToOrderStatePaid = "triggerToOrderStatePaid";

                var state = order.State.AsState(b =>
                {
                    b.From(OrderState.Initial).To(OrderState.Paid)
                        .Changing(OnTransitioningStateFromInitialToPaidHandler);

                    b.TriggerTo(OrderState.Paid, triggerToOrderStatePaid);
                });

                StateMachineManager.Trigger(triggerToOrderStatePaid);

                Assert.Equal(OrderState.Paid, order.State);
            }
            
            public void HandleOrderUsingTrigger(Order order)
            {
                var trigger = new StateChangeTrigger<OrderState>
                    {
                        Uid = order.Uid,
                        NextState = OrderState.Paid
                    };
                
                StateMachineManager.Trigger(trigger);

                Assert.Equal(OrderState.Paid, order.State);
            }


            private void OnTransitioningStateFromInitialToPaidHandler(IState<OrderState> state, OrderState next)
            {
                //throwing exception in Changing handler prevents the state change
                //throw new Exception();        
            }

            private void OnTransitionedStateFromInitialToPaidHandler(OrderState previous, IState<OrderState> state)
            {
            }

            private void ValidateRequest(PaymentGuardContext context)
            {
                //must set Continue to true to proceed to the next handler in the pipeline
                context.Continue = true;
            }

            private void PayToPaymentGateway(PaymentGuardContext context)
            {
                //must set Continue to true to proceed to the next handler in the pipeline
                context.Continue = true;
            }

            private void PersistOrderToRepository(PaymentGuardContext context)
            {
                //must set Continue to true to proceed to the next handler in the pipeline
                context.Continue = true;
            }
        }

    }
}