using System;

namespace StateBliss
{
    public delegate void StateChangeHandler<TState, TTriggerContext>(StateChangeInfo<TState, TTriggerContext> changeInfo)
        where TState : Enum;
    
    public delegate void StateChangeHandler<TState>(StateChangeInfo<TState> changeInfo)
        where TState : Enum;

    public class StateChangeInfo<TState> : StateChangeInfo<TState, object>
        where TState : Enum
    {
    }

    public class StateChangeInfo<TState, TTriggerContext> : StateChangeInfo
        where TState : Enum
    {
        public new TTriggerContext Data { get => (TTriggerContext)base.Data;
            set => base.Data = value;
        }
        
        public new TState FromState { get => base.FromState.ToEnum<TState>(); set => base.FromState = value.ToInt(); }
        public new TState ToState  { get => base.ToState.ToEnum<TState>(); set => base.ToState = value.ToInt(); }
    }
}