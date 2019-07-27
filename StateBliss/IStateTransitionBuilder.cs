using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> OnTransitioned<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler);
        IStateTransitionBuilder<TState> OnTransitioning<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler);
    }
}