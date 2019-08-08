using System;

namespace StateBliss
{
    public abstract class HandlerStateContext : StateContext
    {
        protected HandlerStateContext(Type stateType) : base(stateType)
        {
        }
    }
    
    public class HandlerStateContext<TState> : HandlerStateContext
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

        public HandlerStateContext() : base(typeof(TState))
        {
        }
    }
    
    public class HandlerStateContext<TState, TParentContext> : HandlerStateContext<TState>
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