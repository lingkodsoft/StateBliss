using System;

namespace StateBliss
{
    public delegate void OnGuardHandler<in TContext>(TContext context)
        where TContext : StateContext;
}