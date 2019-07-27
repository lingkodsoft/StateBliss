using System;

namespace StateBliss
{
    public delegate void OnStateTransitionedHandler<TState>(TState previous, IState<TState> state)
        where TState : Enum;
}