namespace StateBliss.SampleApi
{
    public class PayOrderCommand
    {
        public Order Order { get; set; }
        public State<Order, OrderState> State { get; set; }
        public bool GetStateWithGuards { get; set; }
        public int PersistToRepo_CallCount;
        public int PayToGateway_CallCount;
        public int ValidateRequest_CallCount;
        public bool Succeeded { get; set; }
    }
}