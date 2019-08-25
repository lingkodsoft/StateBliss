using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateFromBuilder<TState> where TState : Enum
    {
        IStateToBuilder<TState> From(TState state);
        void OnEntering<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler)
            where T : class;
        void OnEntered<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler)
            where T : class;
        void OnExiting<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler)
            where T : class;
        void OnExited<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler)
            where T : class;
        void OnEditing<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler)
            where T : class;
        void OnEdited<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler)
            where T : class;
        void DisableSameStateTransitionFor(params TState[] states);
        bool ThrowExceptionWhenDiscontinued { get; set; }
    }
}