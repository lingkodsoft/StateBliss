namespace StateBliss.Tests
{
    public class PayOrderCommand
    {
        public Order Order { get; set; }
        public State<Order, OrderState> State { get; set; }
        public bool Succeeded { get; set; }
    }
}