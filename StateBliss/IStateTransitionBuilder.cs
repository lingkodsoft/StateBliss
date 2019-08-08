using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> Changed(OnStateTransitionedHandler<TState> handler);
        IStateTransitionBuilder<TState> Changed<TContext>(ITriggerInfo<TContext> handlers) where TContext : ParentStateContext;
        IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler) where T : class;
        IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler);
        IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler) where T : class;
        IStateTransitionBuilder<TState> Changing<TContext>(IGuardsInfo<TContext> guards)
            where TContext : GuardStateContext<TState>, new();
        IStateTransitionBuilder<TState> TriggeredBy(string trigger);
    }
}