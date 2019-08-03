using System;
using System.Collections.Generic;

namespace StateBliss
{
    public class GuardsInfo<TState, TContext>
        where TState : Enum
        where TContext : GuardContext<TState>
    {
        private readonly Func<TContext> _contextProvider;
        private readonly TContext _context;

        public GuardsInfo(TContext context, IEnumerable<OnGuardHandler<TState, TContext>> guards)
        {
            Guards = guards;
            _context = context;
        }
        
        public GuardsInfo(Func<TContext> contextProvider, IEnumerable<OnGuardHandler<TState, TContext>> guards)
        {
            _contextProvider = contextProvider;
            Guards = guards;
        }
            
        public IEnumerable<OnGuardHandler<TState, TContext>> Guards { get; }

        public TContext Context => _context ?? _contextProvider?.Invoke();
    }
}