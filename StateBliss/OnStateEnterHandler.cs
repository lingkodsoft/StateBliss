using System;

namespace StateBliss
{
    public delegate void OnStateEnterHandler<TState>(TState next, IState<TState> state) where TState : Enum;
    
    public delegate void OnStateEnterHandler<TState, in TContext>(TState next, IState<TState> state, TContext context)
        where TContext : ParentStateContext
        where TState : Enum;
}