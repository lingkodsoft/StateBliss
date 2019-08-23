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

    public class StateChangeInfo<TState, TTriggerContext>
        where TState : Enum
    {
        public object Id { get; set; }
        public object FindResultContext { get; set; }
        public TState FromState { get; set; }
        public TState ToState { get; set; }
    }
}