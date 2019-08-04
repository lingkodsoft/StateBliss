using System;
using System.Collections.Generic;
using System.Linq;

namespace StateBliss.SampleApi
{
    public class StateProvider
    {
        private readonly IEnumerable<IStateDefinition> _stateDefinitions;

        public StateProvider(IEnumerable<IStateDefinition> stateDefinitions)
        {
            _stateDefinitions = stateDefinitions;
        }
        
        public State StatesProvider(Type stateType, Guid id)
        {
            return _stateDefinitions.Single(a => a.EnumType == stateType).DefineState(id);
        }
    }
}