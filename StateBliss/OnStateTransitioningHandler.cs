using System;

namespace StateBliss
{
    public delegate void OnStateTransitioningHandler<TState>(IState<TState> state, TState next) 
        where TState : Enum;
    
    public delegate void OnStateTransitioningHandler<TState, in TContext>(IState<TState> state, TState next, TContext context)
        where TContext : ParentStateContext
        where TState : Enum;
}