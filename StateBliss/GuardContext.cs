using System;

namespace StateBliss
{
    public class GuardContext<TState>
        where TState : Enum
    {
        public TState NextState { get; internal set; }
        public IState<TState> State { get; internal set; }
        
        /// <summary>
        /// When Continue is set to false on a handler, setting ThrowExceptionWhenDiscontinued to true will throw StateEnterGuardDiscontinuedException.
        /// </summary>
        public bool ThrowExceptionWhenDiscontinued { get; set; }
        public bool Continue { get; set; }
    }
}