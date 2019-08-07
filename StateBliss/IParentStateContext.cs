using System;

namespace StateBliss
{
    public abstract class ParentStateContext
    {
        internal StateContext Context { get; set; }
    }
    
    public abstract class ParentStateContext<TState> : ParentStateContext
        where TState : Enum
    {
        internal new StateContext<TState> Context { get; set; }
    }
}