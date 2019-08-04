namespace StateBliss.SampleApi
{
    public class PaymentGuardContext : GuardContext<OrderState>
    {
        public PayOrderCommand Command { get; set; }
    }
}