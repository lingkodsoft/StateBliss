using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler)
            where T : class;
        IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler)
            where T : class;
    }
}