using System;

namespace StateBliss
{
//    public delegate void OnGuardHandler<TState, in TContext>(TContext context)
//        where TState : Enum
//        where TContext : GuardContext<TState>;
//    
    public delegate void OnGuardHandler<in TContext>(TContext context)
        where TContext : GuardContext;
}