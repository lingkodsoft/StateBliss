using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StateBliss
{
    public class State<TEntity, TState> : State<TState> where TState : Enum
    {
        private readonly PropertyInfo _entityStatePropInfo;
        private readonly PropertyInfo _entityUidPropInfo;

        public State(Expression<Func<TEntity, Guid>> uidPropertySelector,
            Expression<Func<TEntity, TState>> statePropertySelector, string name = null,
            bool registerToDefaultStateMachineManager = true)
            : this(default, uidPropertySelector, statePropertySelector,  name, registerToDefaultStateMachineManager)
        {
        }

        public State(TEntity entity, Expression<Func<TEntity, Guid>> uidPropertySelector, 
            Expression<Func<TEntity, TState>> statePropertySelector, string name = null,
            bool registerToDefaultStateMachineManager = true) 
            : base(default, name, registerToDefaultStateMachineManager)
        {
            var entityType = typeof(TEntity);
            var entityStateName = statePropertySelector.GetFieldName();
            _entityStatePropInfo = entityType.GetProperty(entityStateName);
            
            var entityUidName = uidPropertySelector.GetFieldName();
            _entityUidPropInfo = entityType.GetProperty(entityUidName);

            if (entity != null)
            {
                SetEntityInternal(entity);
                Id = (Guid) _entityUidPropInfo.GetValue(entity);
            }
        }

        public override TState Current => GetEntityState();

        private TState GetEntityState() => (TState)_entityStatePropInfo.GetValue(GetEntity());
        
        public override void SetEntity(object entity)
        {
            SetEntityInternal(entity);
        }
        
        private void SetEntityInternal(object entity)
        {
            if (entity == null) throw new Exception($"{nameof(entity)} must not be null");
            Id = (Guid)_entityUidPropInfo.GetValue(entity);
            base.SetEntity(entity);
        }

        internal override void SetEntityState(int newState)
        {
            var state = newState.ToEnum<TState>();
            _entityStatePropInfo.SetValue(GetEntity(), state);
        }

        public TEntity Entity => (TEntity)GetEntity();

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
            EnsureDefinitionExists();
            return Manager.ChangeState(this, newState.ToEnum<TState>());
        }
        
        public bool ChangeTo(TState newState, TriggerCommand command)
        {
            EnsureDefinitionExists();
            StateTransitionBuilder.SetCommand(command);
            return Manager.ChangeState(this, newState);
        }
        
        public bool ChangeTo(TState newState, TriggerCommand command, StateContext<TState> context)
        {
            EnsureDefinitionExists();
            StateTransitionBuilder.SetContext(context);
            StateTransitionBuilder.SetCommand(command);
            return Manager.ChangeState(this, newState);
        }
        
//        public void GuardsForEntry<TContext>(TState state, IHandlersInfo<TContext> handlerInfo)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            StateTransitionBuilder.AddOnStateEnteringGuards(state.ToInt(), handlerInfo.Context,  handlerInfo.Guards);
//        }
//        
//        public void GuardsForExit<TContext>(TState state, IHandlersInfo<TContext> handlerInfo)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            StateTransitionBuilder.AddOnStateExitingGuards(state.ToInt(), handlerInfo.Context,  handlerInfo.Guards);
//        }
//
//        public void GuardsForEdit<TContext>(TState state, IHandlersInfo<TContext> handlerInfo)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            StateTransitionBuilder.AddOnStateEditingGuards(state.ToInt(), handlerInfo.Context, handlerInfo.Guards);
//        }
//
//        public void GuardsForChanging<TContext>(TState fromState, TState toState, IHandlersInfo<TContext> handlerInfo)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            StateTransitionBuilder.AddOnStateChangingGuards(fromState.ToInt(), toState.ToInt(), handlerInfo.Context, handlerInfo.Guards);
//        }
        
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
        private volatile int _current;
        private object Entity;
        
        protected State(Guid id)
        {
            Id = id;            
        }

        public int Current
        {
            get => _current;
            protected set => Interlocked.Exchange(ref _current, value);
        }

        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public IStateMachineManager Manager { get; internal set; }
        internal abstract void SetEntityState(int newState);
        
        internal StateTransitionBuilder StateTransitionBuilder { get; set; }

        internal abstract bool ChangeTo(int newState);

        protected virtual object GetEntity() => Entity;

        public virtual void SetEntity(object entity)
        {
            Entity = entity;
        }
    }
}