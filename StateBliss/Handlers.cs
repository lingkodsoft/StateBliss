using System;

namespace StateBliss
{
    public static class Handlers
    {
        public static IHandlersInfo<TContext> From<TContext>(params OnStateEventHandler<TContext>[] actions)
            where TContext : StateContext
        {
            return new HandlersInfo<TContext>(actions);
        }
        
        public static IHandlersInfo<TContext> From<TContext>(TContext context, 
            params OnStateEventHandler<TContext>[] actions)
            where TContext : StateContext
        {
            return new HandlersInfo<TContext>(context, actions);
        }
        
        public static IHandlersInfo<TContext> From<TContext>(Func<TContext> contextProvider, 
            params OnStateEventHandler<TContext>[] actions)
            where TContext : StateContext
        {
            return new HandlersInfo<TContext>(contextProvider, actions);
        }
    }
}