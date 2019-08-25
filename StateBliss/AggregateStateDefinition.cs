using System;
using System.Linq;

namespace StateBliss
{
    internal class AggregateStateDefinition<TState> : StateDefinition<TState> where TState : Enum
    {
        public AggregateStateDefinition(IStateDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                foreach (var transition in definition.Transitions)
                {
                    AddTransition(transition);
                }
                
                AddDisabledSameStateTransitions(definition.DisabledSameStateTransitions.ToArray());
            }
        }
        
        public override void Define(IStateFromBuilder<TState> builder)
        {
            throw new NotImplementedException();
        }
    }
}