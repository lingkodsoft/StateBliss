using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateFromBuilder<TState> where TState : Enum
    {
        IStateToBuilder<TState> From(TState state);

        void OnEntered<TTriggerCommand>(TState state, OnStateEventHandler<TState, TTriggerCommand> handler)
            where TTriggerCommand : TriggerCommand<TState>;
        
        void OnEntered(TState state, OnStateEventHandler<TState, TriggerCommand<TState>> handler);
        
        void OnEntered<TTriggerCommand, TContext>(TState state, Func<TContext> contextProvider, 
            params OnStateEventHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new();
       
        void OnEntered<TTriggerCommand>(TState state, 
            params OnStateEventHandler<TState, TTriggerCommand, StateContext<TState>>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>, new();
        
        void OnEntered<TTriggerCommand, T>(TState state, T target, Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand>>> handler)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>;
        
        void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEventHandler<TState, TriggerCommand<TState>>>> handler);

        void OnEntered<TTriggerCommand, T, TContext>(TState state, T target, Func<TContext> contextProvider,
            params Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new();






















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