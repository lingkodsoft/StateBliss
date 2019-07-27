using System;
using System.Linq.Expressions;
using System.Reflection;

namespace StateBliss
{
    public abstract class State
    {
        public string Name { get; protected set; }
        public IStateMachineManager Manager { get; internal set; }
        internal abstract void SetEntityState(int newState);
    }

    public class State<TEntity, TState> : State, IState<TState> where TState : Enum
    {
        private readonly TEntity _entity;
        private readonly PropertyInfo _entityPropInfo;

        public State(TEntity entity, Expression<Func<TEntity, TState>> statePropertySelector, string name = null)
        {
            _entity = entity;
            var entityStateName = statePropertySelector.GetFieldName();
            _entityPropInfo = _entity.GetType().GetProperty(entityStateName);
            Name = name;
        }

        internal StateTransitionBuilder<TState> StateTransitionBuilder { get; private set; }

        public TState[] GetNextStates()
        {
            return StateTransitionBuilder.GetNextStates(Current);
        }

        public TState Current => GetEntityState();

        private TState GetEntityState()
        {
            return (TState)_entityPropInfo.GetValue(_entity);
        }

        internal override void SetEntityState(int newState)
        {
            var state = (TState)(object)newState;
            _entityPropInfo.SetValue(_entity, state);
        }

        public TEntity Entity => _entity;

        object IState<TState>.Entity => Entity;

        public State<TEntity, TState> Define(Action<IStateFromBuilder<TState>> builderAction)
        {
            StateTransitionBuilder = new StateTransitionBuilder<TState>();
            builderAction(StateTransitionBuilder);
            return this;
        }
        
        public void ChangeTo(TState newState)
        {
            Manager.ChangeState(this, newState);
        }
    }
}