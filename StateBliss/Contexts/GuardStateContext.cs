using System;

namespace StateBliss
{
    public abstract class GuardStateContext : StateContext
    {
        /// <summary>
        /// When Continue is set to false on a handler, setting ThrowExceptionWhenDiscontinued to true will throw StateEnterGuardDiscontinuedException.
        /// </summary>
        public bool ThrowExceptionWhenDiscontinued { get; set; }

        public bool Continue { get; set; } = true;

        protected GuardStateContext(Type stateType) : base(stateType)
        {
        }
    }
    
    public class GuardStateContext<TState> : GuardStateContext
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

        public GuardStateContext() : base(typeof(TState))
        {
        }
    }
//    
//    public class GuardStateContext<TState, TParentContext> : GuardStateContext<TState>
//        where TParentContext : TriggerCommand<TState>
//        where TState : Enum
//    {
//        public new TParentContext ParentContext
//        {
//            get => (TParentContext)base.ParentContext;
//            internal set => base.ParentContext = value;
//        }
//    }
}