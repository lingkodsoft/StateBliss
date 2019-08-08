using System;

namespace StateBliss
{
    public delegate void OnStateEnterHandler<TState>(TState next, IState<TState> state) where TState : Enum;
}