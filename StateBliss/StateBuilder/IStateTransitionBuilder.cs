using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> Changed<TTriggerCommand>(OnStateEventHandler<TState, TTriggerCommand> handler)
            where TTriggerCommand : TriggerCommand<TState>;
        
        IStateTransitionBuilder<TState> Changed(OnStateEventHandler<TState, TriggerCommand<TState>> handler);
        
        IStateTransitionBuilder<TState> Changed<TTriggerCommand, TContext>(Func<TContext> contextProvider, 
            params OnStateEventHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new();

//        
//        IStateTransitionBuilder<TState> Changed<TContext>(IHandlersInfo<TState, TContext> handlers) 
//            where TContext : HandlerStateContext<TState>, new();
//        
        
//        IStateTransitionBuilder<TState> Changed<TContext>(IHandlersInfo<TContext> handlers) where TContext : HandlerStateContext, new();
//        IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler) where T : class;
//        IStateTransitionBuilder<TState> Changed<TContext>(OnStateEventHandler<TContext> handler) where TContext : HandlerStateContext, new();
//        IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler);
//        IStateTransitionBuilder<TState> Changing<TContext>(OnStateEventHandler<TContext> handler) where TContext : HandlerStateContext, new();
//        IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler) where T : class;
//        IStateTransitionBuilder<TState> Changing<TContext>(IHandlersInfo<TContext> handlers) where TContext : GuardStateContext<TState>, new();
        IStateTransitionBuilder<TState> TriggeredBy(string trigger);
    }
}