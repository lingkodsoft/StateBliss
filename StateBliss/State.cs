using System;
using System.Collections;
using System.Linq.Expressions;

namespace StateBliss
{
    public class State<TEntity, TStatus> : IState
    {
        private readonly TEntity _entity;
        private readonly Expression<Func<TEntity, TStatus>> _stateSelector;

        private IEnumerable<ActionInfo> _transitionActions 
        
        public State(TEntity entity, Expression<Func<TEntity, TStatus>> stateSelector)
        {
            _entity = entity;
            _stateSelector = stateSelector;
        }
        
        public TStatus[] GetNextStates()
        {
            throw new System.NotImplementedException();
        }
        
        public IStateMachineManager Manager { get; internal set; }
        
        public TStatus Current { get; private set; }
        public TEntity Entity => _entity;

        public State<TEntity, TStatus> Define(Action<StateTransitionBuilder> builderAction)
        {
            
            return this;
        }
        
        public State<TEntity, TStatus> Change(TStatus newState)
        {
            Manager.ChamgeState(this, newState);
            Current = newState;
            return this;
        }
    }
}