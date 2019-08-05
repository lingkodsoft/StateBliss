namespace StateBliss.SampleApi.Controllers
{
    public class PayOrderTriggerContext : StateContext<OrderState>
    {
        public Order Order { get; set; }
    }
}