using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public interface IStateTransitionBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> Changing<TTriggerCommand>(OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>> handler)
            where TTriggerCommand : TriggerCommand<TState>;
        
        IStateTransitionBuilder<TState> Changing(OnStateEventGuardHandler<TState, TriggerCommand<TState>> handler);


        IStateTransitionBuilder<TState> Changing<TTriggerCommand>(
            params OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>;

        
        IStateTransitionBuilder<TState> Changing<TTriggerCommand, TContext>(Func<TContext> contextProvider, 
            params OnStateEventGuardHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TContext : GuardStateContext<TState>, new()
            where TTriggerCommand : TriggerCommand<TState>, new();
       
        IStateTransitionBuilder<TState> Changing<T, TTriggerCommand>(T target, Expression<Func<T, OnStateEventGuardHandler<TState, TTriggerCommand>>> handler)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>;
        
        IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, OnStateEventGuardHandler<TState, TriggerCommand<TState>>>> handler)
            where T : class;
        
        IStateTransitionBuilder<TState> Changing<T, TTriggerCommand, TContext>(T target, Func<TContext> contextProvider, 
            params Expression<Func<T, OnStateEventGuardHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>, new()
            where TContext : GuardStateContext<TState>, new();
        
        
        
        
        
        IStateTransitionBuilder<TState> Changed<TTriggerCommand>(OnStateEventHandler<TState, TTriggerCommand> handler)
            where TTriggerCommand : TriggerCommand<TState>;
        
        IStateTransitionBuilder<TState> Changed(OnStateEventHandler<TState, TriggerCommand<TState>> handler);
        
        IStateTransitionBuilder<TState> Changed<TTriggerCommand, TContext>(Func<TContext> contextProvider, 
            params OnStateEventHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new();
       
        IStateTransitionBuilder<TState> Changed<T, TTriggerCommand>(T target, Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand>>> handler)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>;
        
        IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, OnStateEventHandler<TState, TriggerCommand<TState>>>> handler);
        
        IStateTransitionBuilder<TState> Changed<T, TTriggerCommand, TContext>(T target, Func<TContext> contextProvider, 
            params Expression<Func<T,OnStateEventHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new();

        IStateTransitionBuilder<TState> TriggeredBy(string trigger);
    }
}