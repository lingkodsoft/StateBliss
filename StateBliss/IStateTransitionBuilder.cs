using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder
    {
        IStateTransitionBuilder OnEnter<T>(T target, Expression<Func<T, Action>> func);
        IStateTransitionBuilder OnExit<T>(T target, Expression<Func<T, Action>> func);
    }
}