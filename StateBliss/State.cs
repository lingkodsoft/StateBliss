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
            if (StateTransitionBuilder != null)
            {
                throw new InvalidOperationException($"Define must be called once only.");
            }
            StateTransitionBuilder = new StateTransitionBuilder<TState>();
            builderAction(StateTransitionBuilder);
            return this;
        }
        
        public bool ChangeTo(TState newState)
        {
            EnsureDefinitionExists();
            return Manager.ChangeState(this, newState);
        }

        public bool ChangeTo<TContext>(TState newState, GuardsInfo<TState, TContext> guards)
            where TContext : GuardContext<TState>
        {
            EnsureDefinitionExists();

            StateTransitionBuilder.AddOnStateEnterGuards(newState, guards.Context, guards.Guards);
            
            return Manager.ChangeState(this, newState);
        }
        
        private void EnsureDefinitionExists()
        {
            if (StateTransitionBuilder == null)
            {
                throw new InvalidOperationException("Must define states first by calling State.Define method.");
            }
        }
    }
}