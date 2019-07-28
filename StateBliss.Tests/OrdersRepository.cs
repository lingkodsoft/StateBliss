using System;
using System.Collections.Generic;
using System.Linq;

namespace StateBliss.Tests
{
    public class OrdersRepository
    {
        private readonly List<Order> _orders = new List<Order>();
        
        public IReadOnlyList<Order> GetOrders()
        {
            return _orders;
        }
        
        public Order GetOrder(int id)
        {
            return _orders.FirstOrDefault(a => a.Id == id);
        }
        
        public void InsertOrder(Order order)
        {
            _orders.Add(order);
        }
        
        public void UpdateOrder(Order order)
        {
            var existingOrder = _orders.FirstOrDefault(a => a.Id == order.Id);
            if (existingOrder == null)
            {
                throw new Exception($"Order {order.Id} does not exists.");
            }
            _orders.Remove(existingOrder);
            _orders.Add(order);
        }
    }
}