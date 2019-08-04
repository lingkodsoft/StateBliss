using System;

namespace StateBliss.SampleApi
{
    public class Order
    {
        public int Id { get; set; }
        public OrderState State { get; set; }
        public Guid Uid { get; set; }
    }
}