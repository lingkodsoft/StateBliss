using System;

namespace StateBliss
{
    public delegate void OnStateExitHandler<TState>(TState previous, IState<TState> state) where TState : Enum;
}