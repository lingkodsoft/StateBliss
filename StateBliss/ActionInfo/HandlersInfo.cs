using System;
using System.Collections.Generic;

namespace StateBliss
{
    public interface IGuardsInfoForContext<out TContext>
        where TContext : StateContext
    {
        TContext Context { get; }
    }
    
    public interface IHandlersInfo<TContext> : IGuardsInfoForContext<TContext>
        where TContext : StateContext
    {
        IEnumerable<OnStateEventHandler<TContext>> Guards { get; }
        Func<TContext> ContextProvider { get; }
    }

    public class HandlersInfo<TContext> : IHandlersInfo<TContext>
        where TContext : StateContext
    {
        private readonly Func<TContext> _contextProvider;
        private readonly TContext _context;

        public HandlersInfo(IEnumerable<OnStateEventHandler<TContext>> guards)
        {
            Guards = guards;
        }
        
        public HandlersInfo(TContext context, IEnumerable<OnStateEventHandler<TContext>> guards)
        {
            Guards = guards;
            _context = context;
        }
        
        public HandlersInfo(Func<TContext> contextProvider, IEnumerable<OnStateEventHandler<TContext>> guards)
        {
            _contextProvider = contextProvider;
            Guards = guards;
        }
            
        public IEnumerable<OnStateEventHandler<TContext>> Guards { get; }
        public Func<TContext> ContextProvider => () => _context;

        public TContext Context => _context ?? _contextProvider?.Invoke();
    }
}