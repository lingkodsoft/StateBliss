using System;

namespace StateBliss
{
    public delegate void OnStateEnterGuardHandler<TState, in TContext>(TContext context)
        where TState : Enum
        where TContext : GuardContext<TState>;
}