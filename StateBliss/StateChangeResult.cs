using System;

namespace StateBliss
{
    public class StateChangeResult<TState, TData>
        where TState : Enum
    {
        public TData Data { get; internal set; }
        public TState FromState { get; internal set; }
        public TState ToState { get; internal set; }
        public TState CurrentState { get; internal set; }
        public bool StateChangedSucceeded { get; internal set; }
    }
}