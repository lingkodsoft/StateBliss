using System;
using System.Collections.Generic;

namespace StateBliss
{
    public abstract class StateContext
    {
        protected StateContext(Type stateType)
        {
            StateType = stateType;
        }
        
        public Guid Uid { get; set; }
        public bool ChangeStateSucceeded { get; set; }
        
        public Type StateType { get; private set; }
        
        private Dictionary<string, object> _data;
        public int FromState { get; internal set; }
        public int ToState { get; internal set; }
        public State State { get; internal set; }
        
        /// <summary>
        /// When Continue is set to false on a handler, setting ThrowExceptionWhenDiscontinued to true will throw StateEnterGuardDiscontinuedException.
        /// </summary>
        public bool ThrowExceptionWhenDiscontinued { get; set; }
        public bool Continue { get; set; }

        public Dictionary<string, object> Data
        {
            get => _data ?? (_data = new Dictionary<string, object>());
        }
        
        public ParentStateContext ParentContext { get; internal set; }
    }
    
    public class StateContext<TState> : StateContext
        where TState : Enum
    {
        public new TState FromState
        {
            get => base.FromState.ToEnum<TState>();
            internal set => base.FromState = value.ToInt();
        }

        public new TState ToState
        {
            get => base.ToState.ToEnum<TState>();
            set => base.ToState = value.ToInt();
        }
        
        public IState<TState> State{
            get => (IState<TState>)base.State;
            internal set => base.State = (State)value;
        }

        public StateContext() : base(typeof(TState))
        {
        }
    }

    public class StateContext<TState, TParentContext> : StateContext<TState>
        where TParentContext : ParentStateContext<TState>
        where TState : Enum
    {
        public new TParentContext ParentContext
        {
            get => (TParentContext)base.ParentContext;
            internal set => base.ParentContext = value;
        }
    }
}