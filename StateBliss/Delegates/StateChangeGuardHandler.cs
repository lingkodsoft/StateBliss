using System;

namespace StateBliss
{
    public delegate void StateChangeGuardHandler<TState>(StateChangeGuardInfo<TState> changeInfo)
        where TState : Enum;
}