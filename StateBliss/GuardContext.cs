using System;
using System.Collections.Generic;

namespace StateBliss
{
    public class GuardContext<TState>
        where TState : Enum
    {
        private Dictionary<string, object> _data;
        public TState FromState { get; internal set; }
        public TState ToState { get; internal set; }
        public IState<TState> State { get; internal set; }
        
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
}