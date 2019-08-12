using StateBliss.SampleApi.Controllers;

namespace StateBliss.SampleApi
{
    public class PaymentCommand : TriggerCommand<OrderState>
    {
        public int ValidateRequest_CallCount;
        public int PayToGateway_CallCount;
        public int PersistToRepo_CallCount;
    }
    
    public class OtherCommand : TriggerCommand<OrderState>
    {
        
    }
}