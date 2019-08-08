using StateBliss.SampleApi.Controllers;

namespace StateBliss.SampleApi
{
    public class PaymentHandlerContext : HandlerStateContext<OrderState, PayOrderChangeTrigger>
    {
        public int ValidateRequest_CallCount;
        public int PayToGateway_CallCount;
        public int PersistToRepo_CallCount;
    }
}