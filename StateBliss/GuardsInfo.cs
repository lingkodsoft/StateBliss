using System;
using System.Collections.Generic;

namespace StateBliss
{
    public class GuardsInfo<TState, TContext>
        where TState : Enum
        where TContext : GuardContext<TState>
    {
        public GuardsInfo(TContext context, IEnumerable<OnStateEnterGuardHandler<TState, TContext>> guards)
        {
            Guards = guards;
            Context = context;
        }
            
        public IEnumerable<OnStateEnterGuardHandler<TState, TContext>> Guards { get; }
        public TContext Context { get; }
    }
}