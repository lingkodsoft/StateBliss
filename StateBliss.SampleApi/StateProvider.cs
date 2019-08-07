using System;
using System.Collections.Generic;
using System.Linq;

namespace StateBliss.SampleApi
{
    public class StateProvider
    {
        private readonly IEnumerable<IStateDefinition> _stateDefinitions;
        private readonly OrdersRepository _ordersRepository;

        public StateProvider(IEnumerable<IStateDefinition> stateDefinitions, OrdersRepository ordersRepository)
        {
            _stateDefinitions = stateDefinitions;
            _ordersRepository = ordersRepository;
        }
        
        public State StatesProvider(Type stateType, Guid id)
        {
            var state = _stateDefinitions.Single(a => a.EnumType == stateType).DefineState();

            if (stateType == typeof(OrderState))
            {
                var order = _ordersRepository.GetOrders().Single(a => a.Uid == id);
                state.SetEntity(order);
            }
            
            return state;
        }
    }
}