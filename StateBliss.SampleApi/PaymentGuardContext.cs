using StateBliss.SampleApi.Controllers;

namespace StateBliss.SampleApi
{
    public class PaymentGuardContext : StateContext<OrderState, PayOrderTriggerContext>
    {
        public string Data;
        public int ValidateRequest_CallCount;
        public int PayToGateway_CallCount;
        public int PersistToRepo_CallCount;
    }
}