namespace StateBliss.Tests{
    public class OrderStateDefinition
    {
        private readonly IStateMachineManager _stateMachineManager;

        public OrderStateDefinition(IStateMachineManager stateMachineManager)
        {
            _stateMachineManager = stateMachineManager;
        }
        
        public State<Order, OrderState> GetState(Order order)
        {
            var state = new State<Order, OrderState>(order, a => a.State)
                .Define(b =>
                {

                    b.From(OrderState.Initial).To(OrderState.Paid);

                    b.From(OrderState.Paid).To(OrderState.Processed);

                    b.From(OrderState.Processed).To(OrderState.Delivered);

                });

            _stateMachineManager.Register(state);
            return state;
        }
    }
}