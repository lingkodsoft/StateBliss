using System;

namespace StateBliss
{
    public delegate void OnStateTransitioningHandler<TState>(IState<TState> state, TState next) 
        where TState : Enum;
}