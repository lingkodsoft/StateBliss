using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> Changed(OnStateTransitionedHandler<TState> handler);
        IStateTransitionBuilder<TState> Changed<TContext>(IHandlersInfo<TContext> handlers) where TContext : HandlerStateContext, new();
        
        IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler) where T : class;
        IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler);
        IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler) where T : class;
        IStateTransitionBuilder<TState> Changing<TContext>(IHandlersInfo<TContext> handlers) where TContext : GuardStateContext<TState>, new();
        IStateTransitionBuilder<TState> TriggeredBy(string trigger);
    }
}