using System;

namespace StateBliss
{
    public static class Guards<TState>
        where TState : Enum
    {
        public static GuardsInfo<TState, TContext> From<TContext>(TContext context, 
            params OnStateEnterGuardHandler<TState, TContext>[] actions)
            where TContext : GuardContext<TState>
        {
            return new GuardsInfo<TState, TContext>(context, actions);
        }
        
        public static GuardsInfo<TState, TContext> From<TContext>(Func<TContext> contextProvider, 
            params OnStateEnterGuardHandler<TState, TContext>[] actions)
            where TContext : GuardContext<TState>
        {
            return new GuardsInfo<TState, TContext>(contextProvider, actions);
        }
    }
}