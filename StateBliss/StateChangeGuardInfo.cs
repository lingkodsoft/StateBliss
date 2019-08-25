using System;

namespace StateBliss
{
    public class StateChangeGuardInfo<TState> : StateChangeInfo<TState>
        where TState : Enum
    {
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