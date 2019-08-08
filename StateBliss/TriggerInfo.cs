using System;
using System.Collections.Generic;

namespace StateBliss
{
    public class TriggerInfo<TContext> : ITriggerInfo<TContext>
        where TContext : ParentStateContext
    {
        private readonly Func<TContext> _contextProvider;
        private readonly TContext _context;

        public TriggerInfo(IEnumerable<OnTriggerHandler<TContext>> guards)
        {
            Guards = guards;
        }
        
        public TriggerInfo(TContext context, IEnumerable<OnTriggerHandler<TContext>> guards)
        {
            Guards = guards;
            _context = context;
        }
        
        public TriggerInfo(Func<TContext> contextProvider, IEnumerable<OnTriggerHandler<TContext>> guards)
        {
            _contextProvider = contextProvider;
            Guards = guards;
        }
            
        public IEnumerable<OnTriggerHandler<TContext>> Guards { get; }
        public Func<TContext> ContextProvider => () => _context;

        public TContext Context => _context ?? _contextProvider?.Invoke();
    }
}