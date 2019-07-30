# StateBliss : Finite State Machine in c#

[Stackoverflow](https://stackoverflow.com/questions/5923767/simple-state-machine-example-in-c/5924053)

# How to use 


You can do this. See unit tests for more examples.

```
  public class Order
  {
      public int Id { get; set; }
      public OrderState State { get; set; }
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
      private class PaymentGuardContext : GuardContext<OrderState>
      {
      }
      
      public void HandleOrder(Order order)
      {
          var context = new PaymentGuardContext();
      
          var state = new State<Order, OrderState>(order, a => a.State)
                    .Define(b =>
                    {
                        b.From(OrderState.Initial).To(OrderState.Paid)
                          .Changing(OnTransitioningStateFromInitialToPaidHandler)
                          .Changed(OnTransitionedStateFromInitialToPaidHandler);
                        
                        b.From(OrderState.Paid).To(OrderState.Processed);
                        b.From(OrderState.Processed).To(OrderState.Delivered);

                        b.OnEnter(OrderState.Paid, Guards<OrderState>.From(context,
                            ValidateRequest,
                            PayToPaymentGateway, 
                            PersistOrderToRepository
                        ));

                        b.DisableSameStateTransitionFor(OrderState.Paid);
                        
                    });
  
        var hasChangedState = state.ChangeTo(OrderState.Paid);
        
        //Assert.Equal(OrderState.Paid, order.State);
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
        
        //Assert.Equal(OrderState.Paid, order.State);
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
  
  
```

# Install from Nuget

`Install-Package StateBliss -Version 1.0.0`
