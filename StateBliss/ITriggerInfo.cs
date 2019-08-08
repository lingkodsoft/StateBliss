using System;
using System.Collections.Generic;

namespace StateBliss
{
    public interface ITriggerInfo<TContext> : ITriggersInfoForContext<TContext>
        where TContext : ParentStateContext
    {
        IEnumerable<OnTriggerHandler<TContext>> Guards { get; }
        Func<TContext> ContextProvider { get; }
    }
}