namespace StateBliss.SampleApi.Controllers
{
    public class PayOrderChangeTrigger : StateChangeTrigger<OrderState>
    {
        public Order Order { get; set; }
    }
}