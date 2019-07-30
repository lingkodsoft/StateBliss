using System;
using System.Linq.Expressions;
using System.Reflection;

namespace StateBliss
{
    public class State<TEntity, TState> : State<TState> where TState : Enum
    {
        private readonly TEntity _entity;
        private readonly PropertyInfo _entityPropInfo;
        
        public State(TEntity entity, Expression<Func<TEntity, TState>> statePropertySelector, string name = null,
            bool registerToDefaultStateMachineManager = true) 
            : base(default, name, registerToDefaultStateMachineManager)
        {
            _entity = entity;
            var entityStateName = statePropertySelector.GetFieldName();
            _entityPropInfo = _entity.GetType().GetProperty(entityStateName);
        }

        public override TState Current => GetEntityState();

        private TState GetEntityState() => (TState)_entityPropInfo.GetValue(_entity);

        internal override void SetEntityState(int newState)
        {
            var state = (TState)(object)newState;
            _entityPropInfo.SetValue(_entity, state);
        }

        public TEntity Entity => _entity;

        protected override object GetEntity() => Entity;

        public new State<TEntity, TState> Define(Action<IStateFromBuilder<TState>> builderAction)
        {
            base.Define(builderAction);
            return this;
        }
    }
    
    public class State<TState> : State, IState<TState> where TState : Enum
    {
        public State(TState state, string name = null, bool registerToDefaultStateMachineManager = true)
        {
            base.Current = (int)Enum.ToObject(state.GetType(), state);
            Name = name;
            if (registerToDefaultStateMachineManager)
            {
                StateMachineManager.Default.Register(this);
            }
        }

        internal new StateTransitionBuilder<TState> StateTransitionBuilder
        {
            get => (StateTransitionBuilder<TState>)base.StateTransitionBuilder;
            set => base.StateTransitionBuilder = value;
        }

        public TState[] GetNextStates()
        {
            return StateTransitionBuilder.GetNextStates(Current);
        }

        object IState<TState>.Entity => GetEntity();

        protected virtual object GetEntity() => null;

        public new virtual TState Current => (TState)(object)base.Current; 
        
        internal override void SetEntityState(int newState)
        {
            base.Current = newState;
        }

        public State<TState> Define(Action<IStateFromBuilder<TState>> builderAction)
        {
            if (StateTransitionBuilder != null)
            {
                throw new InvalidOperationException($"Define must be called once only.");
            }
            StateTransitionBuilder = new StateTransitionBuilder<TState>(this);
            builderAction(StateTransitionBuilder);
            return this;
        }

        internal override bool ChangeTo(int newState)
        {
            return ChangeTo((TState) (object) newState);
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
    
    public abstract class State
    {
        protected State()
        {
            Id = Guid.NewGuid();            
        }
        
        public int Current { get; protected set; }
        
        public Guid Id { get; private set; }
        public string Name { get; protected set; }
        public IStateMachineManager Manager { get; internal set; }
        internal abstract void SetEntityState(int newState);
        
        internal StateTransitionBuilder StateTransitionBuilder { get; set; }

        internal abstract bool ChangeTo(int newState);
        
    }
}