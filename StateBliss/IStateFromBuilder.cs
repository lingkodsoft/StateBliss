using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateFromBuilder<TState> where TState : Enum
    {
        IStateToBuilder<TState> From(TState state);
        void OnEntered(TState state, OnStateEnterHandler<TState> handler);
        void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler);
        void OnEntering<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>;
        void OnExited(TState state, OnStateExitHandler<TState> handler);
        void OnExited<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler);
        void OnExiting<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>;
        void OnEdited(TState state, OnStateEnterHandler<TState> handler);
        void OnEdited<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler);
        void OnEditing<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>;
        void DisableSameStateTransitionFor(params TState[] states);
        void TriggerTo(TState nextState, string trigger);
    }
}