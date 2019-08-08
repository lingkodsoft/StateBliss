using System;

namespace StateBliss
{
    public static class Handlers
    {
        public static ITriggerInfo<TContext> From<TContext>(params OnTriggerHandler<TContext>[] actions)
            where TContext : ParentStateContext
        {
            return new TriggerInfo<TContext>(actions);
        }
    }
}