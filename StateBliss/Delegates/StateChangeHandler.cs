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
    
    public class StateChangeInfo
    {
        internal object TriggerContext { get; set; }
        internal int FromState { get; set; }
        internal int ToState { get; set; }
        
        /// <summary>
        /// When Continue is set to false on a handler, setting ThrowExceptionWhenDiscontinued to true will throw StateEnterGuardDiscontinuedException.
        /// Only applies to Changing, OnEntering and OnExiting transitions
        /// </summary>
        public bool ThrowExceptionWhenDiscontinued { get; set; }

        /// <summary>
        /// Only applies to Changing, OnEntering and OnExiting transitions
        /// </summary>
        public bool Continue { get; set; } = true;
        
    }

    public class StateChangeInfo<TState, TTriggerContext> : StateChangeInfo
        where TState : Enum
    {
        public TTriggerContext TriggerContext { get => (TTriggerContext)base.TriggerContext;
            set => base.TriggerContext = value;
        }
        
        public TState FromState { get => base.FromState.ToEnum<TState>(); set => base.FromState = value.ToInt(); }
        public TState ToState  { get => base.ToState.ToEnum<TState>(); set => base.ToState = value.ToInt(); }
    }
}