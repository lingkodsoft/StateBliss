using System;
using System.Collections.Generic;

namespace StateBliss
{
    public interface IGuardsInfoForContext<out TContext>
        where TContext : StateContext
    {
        TContext Context { get; }
    }
    
    public interface IGuardsInfo<TContext> : IGuardsInfoForContext<TContext>
        where TContext : StateContext
    {
        IEnumerable<OnGuardHandler<TContext>> Guards { get; }
        Func<TContext> ContextProvider { get; }
    }

    public class GuardsInfo<TContext> : IGuardsInfo<TContext>
        where TContext : StateContext
    {
        private readonly Func<TContext> _contextProvider;
        private readonly TContext _context;

        public GuardsInfo(IEnumerable<OnGuardHandler<TContext>> guards)
        {
            Guards = guards;
        }
        
        public GuardsInfo(TContext context, IEnumerable<OnGuardHandler<TContext>> guards)
        {
            Guards = guards;
            _context = context;
        }
        
        public GuardsInfo(Func<TContext> contextProvider, IEnumerable<OnGuardHandler<TContext>> guards)
        {
            _contextProvider = contextProvider;
            Guards = guards;
        }
            
        public IEnumerable<OnGuardHandler<TContext>> Guards { get; }
        public Func<TContext> ContextProvider => () => _context;

        public TContext Context => _context ?? _contextProvider?.Invoke();
    }
}