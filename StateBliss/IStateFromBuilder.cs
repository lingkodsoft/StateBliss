using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateFromBuilder<TState> where TState : Enum
    {
        IStateToBuilder<TState> From(TState state);
        void OnEnter(TState state, OnStateEnterHandler<TState> handler);
        void OnEnter<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>;
        void OnEnter<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler);
        void OnExit(TState state, OnStateExitHandler<TState> handler);
        void OnExit<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler);
        void DisableSameStateTransitionFor(params TState[] states);
        
    }
}