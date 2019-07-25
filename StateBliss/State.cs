using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace StateBliss
{
    public abstract class State
    {
        internal void SetCurrentState<TStatus>(TStatus newState) where TStatus : Enum
        {
            throw new NotImplementedException();
        }

        internal IReadOnlyList<ActionInfo> GetOnTransitioningHandlers<TStatus>(TStatus newState) where TStatus : Enum
        {
            throw new NotImplementedException();
        }

        internal IReadOnlyList<ActionInfo> GetOnExitHandlers()
        {
            throw new NotImplementedException();
        }

        internal IReadOnlyList<ActionInfo> GetOnEnterHandlers<TStatus>(TStatus newState) where TStatus : Enum
        {
            throw new NotImplementedException();
        }

        internal IReadOnlyList<ActionInfo> GetOnTransitionedHandlers<TStatus>(TStatus newState) where TStatus : Enum
        {
            throw new NotImplementedException();
        }
    }
    
    public class State<TEntity, TStatus> : State where TStatus : Enum
    {
        private readonly TEntity _entity;
        private readonly Expression<Func<TEntity, TStatus>> _stateSelector;

        private IEnumerable<ActionInfo> _transitionActions;
        
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

        public State<TEntity, TStatus> Define(Action<StateTransitionBuilder<TStatus>> builderAction)
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