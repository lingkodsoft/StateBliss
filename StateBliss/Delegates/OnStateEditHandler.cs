using System;

namespace StateBliss
{
    public delegate void OnStateEditHandler<TState>(TState currentState, IState<TState> state) where TState : Enum;
}