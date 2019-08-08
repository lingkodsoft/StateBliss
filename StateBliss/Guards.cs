using System;

namespace StateBliss
{
    public static class Guards
    {
        public static IGuardsInfo<TContext> From<TContext>(params OnGuardHandler<TContext>[] actions)
            where TContext : GuardStateContext
        {
            return new GuardsInfo<TContext>(actions);
        }
        
        public static IGuardsInfo<TContext> From<TContext>(TContext context, 
            params OnGuardHandler<TContext>[] actions)
            where TContext : GuardStateContext
        {
            return new GuardsInfo<TContext>(context, actions);
        }
        
        public static IGuardsInfo<TContext> From<TContext>(Func<TContext> contextProvider, 
            params OnGuardHandler<TContext>[] actions)
            where TContext : GuardStateContext
        {
            return new GuardsInfo<TContext>(contextProvider, actions);
        }
    }
}