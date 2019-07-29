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

        public GuardsInfo(TContext context, IEnumerable<OnStateEnterGuardHandler<TState, TContext>> guards)
        {
            Guards = guards;
            _context = context;
        }
        
        public GuardsInfo(Func<TContext> contextProvider, IEnumerable<OnStateEnterGuardHandler<TState, TContext>> guards)
        {
            _contextProvider = contextProvider;
            Guards = guards;
        }
            
        public IEnumerable<OnStateEnterGuardHandler<TState, TContext>> Guards { get; }

        public TContext Context => _context ?? _contextProvider?.Invoke();
    }
}