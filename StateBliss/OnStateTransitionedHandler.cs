using System;

namespace StateBliss
{
    public delegate void OnStateTransitionedHandler<TState>(TState previous, IState<TState> state)
        where TState : Enum;
    
    public delegate void OnStateTransitionedHandler<TState, TContext>(TState previous, IState<TState> state, TContext context)
        where TContext : ParentStateContext
        where TState : Enum;
}