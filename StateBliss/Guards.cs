using System;

namespace StateBliss
{
//    public static class Guards<TState>
//        where TState : Enum
//    {
//        public static GuardsInfo<TContext> From<TContext>( 
//            params OnGuardHandler<TState, TContext>[] actions)
//            where TContext : GuardContext<TState>
//        {
//            return new GuardsInfo<TState, TContext>(actions);
//        }
//        
//        public static GuardsInfo<TState, TContext> From<TContext>(TContext context, 
//            params OnGuardHandler<TState, TContext>[] actions)
//            where TContext : GuardContext<TState>
//        {
//            return new GuardsInfo<TState, TContext>(context, actions);
//        }
//        
//        public static GuardsInfo<TState, TContext> From<TContext>(Func<TContext> contextProvider, 
//            params OnGuardHandler<TState, TContext>[] actions)
//            where TContext : GuardContext<TState>
//        {
//            return new GuardsInfo<TState, TContext>(contextProvider, actions);
//        }
//    }

    public static class Guards
    {
        public static IGuardsInfo<TContext> From<TContext>(params OnGuardHandler<TContext>[] actions)
            where TContext : GuardContext
        {
            return new GuardsInfo<TContext>(actions);
        }
        
        public static IGuardsInfo<TContext> From<TContext>(TContext context, 
            params OnGuardHandler<TContext>[] actions)
            where TContext : GuardContext
        {
            return (IGuardsInfo<TContext>)new GuardsInfo<TContext>(context, actions);
        }
        
        public static IGuardsInfo<TContext> From<TContext>(Func<TContext> contextProvider, 
            params OnGuardHandler<TContext>[] actions)
            where TContext : GuardContext
        {
            return new GuardsInfo<TContext>(contextProvider, actions);
        }
    }
}