using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder
    {
        IStateTransitionBuilder OnTransitioned<T>(T target, Expression<Func<T, Action>> func);
        IStateTransitionBuilder OnTransitioning<T>(T target, Expression<Func<T, Action>> func);
    }
}