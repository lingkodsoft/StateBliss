using System;

namespace StateBliss
{
    public delegate void OnStateExitHandler<TState>(TState previous, IState<TState> state) 
        where TState : Enum;
    
    public delegate void OnStateExitHandler<TState, in TContext>(TState previous, IState<TState> state, TContext context)
        where TContext : ParentStateContext
        where TState : Enum;
}