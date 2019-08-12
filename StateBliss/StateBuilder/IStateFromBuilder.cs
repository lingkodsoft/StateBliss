using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateFromBuilder<TState> where TState : Enum
    {
        IStateToBuilder<TState> From(TState state);
        
//        void OnEntered(TState state, OnStateEnterHandler<TState> handler);
//
//        void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
//            where T : class;
//
//        void OnEntered<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new();
//
//        void OnEntering<TContext>(TState state, IHandlersInfo<TContext> guardHandlers)
//            where TContext : GuardStateContext<TState>, new();
//
//        void OnExited(TState state, OnStateExitHandler<TState> handler);
//
//        void OnExited<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler)
//            where T : class;
//
//        void OnExited<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new();
//
//        void OnExiting<TContext>(TState state, IHandlersInfo<TContext> guardHandlers)
//            where TContext : GuardStateContext<TState>, new();
//
//        void OnEdited(TState state, OnStateEnterHandler<TState> handler);
//
//        void OnEdited<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
//            where T : class;
//
//        void OnEdited<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new();
//
//        void OnEditing<TContext>(TState state, IHandlersInfo<TContext> guardHandlers)
//            where TContext : GuardStateContext<TState>, new();

        void DisableSameStateTransitionFor(params TState[] states);
        void TriggerTo(TState nextState, string trigger);
    }
}