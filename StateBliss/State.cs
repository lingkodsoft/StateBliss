using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace StateBliss
{
    public abstract class State
    {
        internal void SetCurrentState<TState>(TState newState) where TState : Enum
        {
            throw new NotImplementedException();
        }

        public IStateMachineManager Manager { get; internal set; }

        internal abstract void SetState(int newState);
    }

    public interface IState<TState> where TState : Enum
    {
        TState Current { get; }
        TState[] GetNextStates();
        IStateMachineManager Manager { get; }
        object Entity { get; }
    }
    
    public class State<TEntity, TState> : State, IState<TState> where TState : Enum
    {
        private readonly TEntity _entity;
        internal Expression<Func<TEntity, TState>> StateSelector { get; private set; }
        internal StateTransitionBuilder<TState> StateTransitionBuilder { get; private set; }

        public State(TEntity entity, Expression<Func<TEntity, TState>> stateSelector)
        {
            _entity = entity;
            StateSelector = stateSelector;
        }
        
        public TState[] GetNextStates()
        {
            throw new System.NotImplementedException();
        }

        public TState Current { get; private set; }

        internal override void SetState(int newState)
        {
            //TODO: set the entity state
            Current = (TState)Enum.ToObject(newState.GetType(), newState);
        }

        public TEntity Entity => _entity;

        object IState<TState>.Entity => Entity;

        public State<TEntity, TState> Define(Action<IStateFromBuilder<TState>> builderAction)
        {
            StateTransitionBuilder = new StateTransitionBuilder<TState>();
            builderAction(StateTransitionBuilder);
            return this;
        }
        
        public State<TEntity, TState> Change(TState newState)
        {
            Manager.ChamgeState(this, newState);
            return this;
        }
    }
}