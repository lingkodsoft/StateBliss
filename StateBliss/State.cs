using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace StateBliss
{
    public abstract class State
    {
        public int Value { get; protected set; }
        public IStateMachineManager Manager { get; protected set; }
        public IStateDefinition Definition { get; protected set; }
    }
    
    public class State<TState> : State
        where TState : Enum
    {
        private readonly StateHandlerDefinition<TState> _stateHandlerDefinition;
        private readonly TState _value;
        private readonly Type _enumType;
        private readonly IStateMachineManager _manager;
        
        internal State(TState state, 
            StateHandlerDefinition<TState> stateHandlerDefinition,
            IStateMachineManager stateMachineManager = null)
        {
            _stateHandlerDefinition = stateHandlerDefinition;
            _value = state;
            _enumType = typeof(TState);
            _manager = stateMachineManager ?? StateMachineManager.Default;
            Definition = _stateHandlerDefinition;
            Manager = stateMachineManager;
            base.Value = state.ToInt();
            // _manager.Register(this);
        }
        
        public TState[] GetNextStates()
        {
            return _stateHandlerDefinition.GetNextStates(Value);
        }

        public new TState Value => _value;

        public StateHandlerDefinition<TState> HandlerDefinition => _stateHandlerDefinition;

        private static bool Equals(State<TState> x, State<TState> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x._value.ToInt() == y._value.ToInt() && x._enumType == y._enumType;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.Value * 397) ^ (_enumType != null ? _enumType.GetHashCode() : 0);
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
}