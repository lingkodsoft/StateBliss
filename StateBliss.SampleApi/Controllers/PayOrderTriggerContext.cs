namespace StateBliss.SampleApi.Controllers
{
    public class PayOrderChangeTrigger : TriggerCommand<OrderState>
    {
        public Order Order { get; set; }
    }
}