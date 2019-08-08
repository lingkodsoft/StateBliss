using System;

namespace StateBliss
{
    public delegate void OnStateEventHandler<in TContext>(TContext context)
        where TContext : StateContext;
}