using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StateBliss
{
    public abstract class State
    {
//        int Current { get; }
//        int[] GetNextStates();
        public IStateMachineManager Manager { get; internal set; }
        public object Id { get; set; }
    }
    
    public class State<TState> : State
        
        //, IState<TState>
        where TState : Enum
    {
        private readonly StateHandlerDefinition<TState> _stateHandlerDefinition;
        private readonly int _current;
        private readonly Type _enumType;
        private readonly IStateMachineManager _manager;
        
        internal State(TState state, 
            StateHandlerDefinition<TState> stateHandlerDefinition,
            IStateMachineManager stateMachineManager = null)
        {
            _stateHandlerDefinition = stateHandlerDefinition;
            _current = state.ToInt();
            _enumType = typeof(TState);
            _manager = stateMachineManager ?? StateMachineManager.Default;
           // _manager.Register(this);
        }
        
        public TState[] GetNextStates()
        {
            return _stateHandlerDefinition.GetNextStates(Current);
        }

        public virtual TState Current
        {
            get => _current.ToEnum<TState>();
//            protected set => Interlocked.Exchange(ref _current, value.ToInt());
        }

        public StateHandlerDefinition<TState> HandlerDefinition => _stateHandlerDefinition;

//        public State<TState> Define(Action<IStateFromBuilder<TState>> builderAction)
//        {
//            if (StateTransitionBuilder != null)
//            {
//                throw new InvalidOperationException($"Define must be called once only.");
//            }
//            StateTransitionBuilder = new StateTransitionBuilder<TState>(this);
//            builderAction(StateTransitionBuilder);
//            return this;
//        }

        
        
        
//        internal State<TState> ChangeTo(int newState)
//        {
//            EnsureDefinitionExists();
//            return _manager.ChangeState(this, newState.ToEnum<TState>());
//        }
//        
//        
//        
//        public State<TState> ChangeTo(TState newState, TriggerCommand command)
//        {
//            EnsureDefinitionExists();
//            StateTransitionBuilder.SetCommand(command);
//            return _manager.ChangeState(this, newState);
//        }
//         
//        public bool ChangeTo(TState newState, TriggerCommand command, StateContext<TState> context)
//        {
//            EnsureDefinitionExists();
//            StateTransitionBuilder.SetContext(context);
//            StateTransitionBuilder.SetCommand(command);
//            return _manager.ChangeState(this, newState);
//        }
        
        private void EnsureDefinitionExists()
        {
            if (_stateHandlerDefinition == null)
            {
                throw new InvalidOperationException("Must define states first by calling State.Define method.");
            }
        }
        
        private static bool Equals(State<TState> x, State<TState> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x._current == y._current && x._enumType == y._enumType;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_current * 397) ^ (_enumType != null ? _enumType.GetHashCode() : 0);
            }
        }

        public static bool operator == (State<TState> x, State<TState> y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(State<TState> x, State<TState> y)
        {
            return !Equals(x, y);
        }

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


}