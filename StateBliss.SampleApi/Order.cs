using System;

namespace StateBliss.SampleApi
{
    public class Order
    {
        public static Guid TestUid = Guid.Parse("3AFEF7E8-2DF2-4245-A2F8-D050DBE6E417");
        
        public int Id { get; set; }
        public OrderState State { get; set; }
        public Guid Uid { get; set; }
    }
}