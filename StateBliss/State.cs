using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StateBliss
{
    public class State<TEntity, TState> : State<TState> where TState : Enum
    {
        private readonly TEntity _entity;
        private readonly PropertyInfo _entityStatePropInfo;
        private readonly PropertyInfo _entityUidPropInfo;
        
        public State(TEntity entity, Expression<Func<TEntity, Guid>> uidPropertySelector, 
            Expression<Func<TEntity, TState>> statePropertySelector, string name = null,
            bool registerToDefaultStateMachineManager = true) 
            : base(default, name, registerToDefaultStateMachineManager)
        {
            _entity = entity;
            var entityStateName = statePropertySelector.GetFieldName();
            _entityStatePropInfo = _entity.GetType().GetProperty(entityStateName);
            
            var entityUidName = uidPropertySelector.GetFieldName();
            _entityUidPropInfo = _entity.GetType().GetProperty(entityUidName);

            Id = (Guid)_entityUidPropInfo.GetValue(_entity);
        }

        public override TState Current => GetEntityState();

        private TState GetEntityState() => (TState)_entityStatePropInfo.GetValue(_entity);

        internal override void SetEntityState(int newState)
        {
            var state = newState.ToEnum<TState>();
            _entityStatePropInfo.SetValue(_entity, state);
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
        public State(TState state) : this(state, null, true)
        {
        }

        public State(TState state, string name = null, bool registerToDefaultStateMachineManager = true)
            : this(Guid.NewGuid(), state, name, registerToDefaultStateMachineManager)
        {
        }
        
        public State(Guid id, TState state, string name = null, bool registerToDefaultStateMachineManager = true)
            :base(id)
        {
            base.Current = state.ToInt();
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

        public new virtual TState Current => base.Current.ToEnum<TState>(); 
        
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
            return ChangeTo(newState.ToEnum<TState>(), null);
        }
        
        public bool ChangeTo(TState newState, StateContext<TState> context = null)
        {
            EnsureDefinitionExists();
            StateTransitionBuilder.SetContext(context);
            return Manager.ChangeState(this, newState);
        }
        
        public void GuardsForEntry<TContext>(TState state, IGuardsInfo<TContext> guardInfo)
            where TContext : StateContext<TState>, new()
        {
            StateTransitionBuilder.AddOnStateEnterGuards(state.ToInt(), guardInfo.Context,  guardInfo.Guards);
        }
        
        public void GuardsForExit<TContext>(TState state, IGuardsInfo<TContext> guardInfo)
            where TContext : StateContext<TState>, new()
        {
            StateTransitionBuilder.AddOnStateExitGuards(state.ToInt(), guardInfo.Context,  guardInfo.Guards);
        }

        public void GuardsForEdit<TContext>(TState state, IGuardsInfo<TContext> guardInfo)
            where TContext : StateContext<TState>, new()
        {
            StateTransitionBuilder.AddOnStateEditGuards(state.ToInt(), guardInfo.Context, guardInfo.Guards);
        }

        public void GuardsForChanging<TContext>(TState fromState, TState toState, IGuardsInfo<TContext> guardInfo)
            where TContext : StateContext<TState>, new()
        {
            StateTransitionBuilder.AddOnStateChangingGuards(fromState.ToInt(), toState.ToInt(), guardInfo.Context, guardInfo.Guards);
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
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private volatile int _current;

        protected State(Guid id)
        {
            Id = id;            
        }

        public int Current
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _current;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            protected set
            {
                _lock.EnterWriteLock();
                try
                {
                    _current = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public IStateMachineManager Manager { get; internal set; }
        internal abstract void SetEntityState(int newState);
        
        internal StateTransitionBuilder StateTransitionBuilder { get; set; }

        internal abstract bool ChangeTo(int newState);
        
    }
}