using System;

namespace StateBliss
{
    
    public class StateChangeInfo<TState> : StateChangeInfo
        where TState : Enum
    {
        public new TState FromState
        {
            get => base.FromState.ToEnum<TState>();
            set => base.FromState = value.ToInt();
        }

        public new TState ToState
        {
            get => base.ToState.ToEnum<TState>();
            set => base.ToState = value.ToInt();
        }
    }

    public class StateChangeInfo
    {
        internal object Data { get; set; }
        public T DataAs<T>() => (T)Data;
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
}