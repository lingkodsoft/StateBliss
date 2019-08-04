using System;
using System.Collections.Generic;

namespace StateBliss
{
    public abstract class GuardContext
    {
        protected GuardContext(Type stateType)
        {
            StateType = stateType;
        }
        
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
    }

    public class GuardContext<TState> : GuardContext
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
            internal set => base.ToState = value.ToInt();
        }
        
        public IState<TState> State{
            get => (IState<TState>)base.State;
            internal set => base.State = (State)value;
        }

        public GuardContext() : base(typeof(TState))
        {
        }
    }
}