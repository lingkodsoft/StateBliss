using System;

namespace StateBliss
{
    public delegate void StateChangeHandler<TState>(StateChangeInfo<TState> changeInfo)
        where TState : Enum;
}