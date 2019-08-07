# StateBliss : Finite State Machine in c#

[Stackoverflow](https://stackoverflow.com/questions/5923767/simple-state-machine-example-in-c/5924053)

# How to use 


You can do this. See unit tests and SampleApi for more examples.

```
  public class Order
  {
      public int Id { get; set; }
      public OrderState State { get; set; }
      public Guid Uid { get; set; }
      public static int TestId = 1;

      public static Guid TestUid = Guid.Parse("3AFEF7E8-2DF2-4245-A2F8-D050DBE6E417");
  }

  public enum OrderState
  {
      Initial,
      Paid,
      Processing,
      Processed,
      Delivered
  }

  public class PayOrderChangeTrigger : StateChangeTrigger<OrderState>
  {
      public Order Order { get; set; }
  }

  //call in your application startup, see SampleApi for example.
  public void SetStateFactory()
  {
      StateMachineManager.Default.SetStateFactory((stateType, uid) =>
      {
          if (stateType == typeof(OrderState))
          {

               var order = _ordersRepository.GetOrders().Single(a => a.Uid == id);            
               
               return new State<Order, OrderState>(order, a => a.Uid, a => a.State)
                  .Define(b =>
                  {
                      b.From(OrderState.Initial).To(OrderState.Paid)
                          .Changing(FromInitialToPaidStateChaningHandler);

                      b.From(OrderState.Paid).To(OrderState.Processing);
                      b.From(OrderState.Processing).To(OrderState.Processed);

                  });
          }
          
          //TODO: return other state here. see SampleApi for example.
          throw new NotImplementedException();
      });
  }
  
  public void TriggerStateChange()
  {
      var uid = Order.TestUid;
      var cmd = new PayOrderChangeTrigger
      {
          Order = new Order
          {
              Id = Order.TestId,
              Uid = uid,
              State = OrderState.Initial
          },
          Uid = uid,
          NextState = OrderState.Paid
      };
      
      StateMachineManager.Trigger(cmd);

      var succeeded = cmd.ChangeStateSucceeded && cmd.State.Current == OrderState.Paid;
  }

  private void FromInitialToPaidStateChaningHandler(IState<OrderState> state, OrderState next)
  {
      //TODO: do some business logic
  }
  
  
}
  
  
```

# Install from Nuget

`Install-Package StateBliss -Version 1.0.1`
