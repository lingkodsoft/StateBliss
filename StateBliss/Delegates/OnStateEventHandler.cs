using System;

namespace StateBliss
{
    public delegate void OnStateEventHandler();

    public delegate void OnStateEventHandler<in TContext>(TContext context)
        where TContext : new();
    
    public delegate void OnStateEventHandler<TState, in TTriggerCommand>(TTriggerCommand command)
        where TState : Enum
        where TTriggerCommand : TriggerCommand<TState>;
    
    public delegate void OnStateEventHandler<TState, in TTriggerCommand, in TContext>(TTriggerCommand command, TContext context)
        where TState : Enum
        where TContext : new()
        where TTriggerCommand : TriggerCommand<TState>;
}