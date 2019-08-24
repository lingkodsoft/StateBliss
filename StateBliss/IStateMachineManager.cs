using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StateBliss
{
    public interface IStateMachineManager : IDisposable
    {
        void Register(IEnumerable<Assembly> assemblyDefinitions, Func<Type, object> serviceProvider = null);
        void Trigger<TState>(TState currentState, TState nextState, object context) where TState : Enum;
        
//        void Register(IEnumerable<IStateDefinition> definitions);
//        State<TState> ChangeState<TState>(State<TState> state, TState newState) where TState : Enum;
//        bool ChangeState<TState>(TState newState, Guid id) where TState : Enum;
//        void Trigger<TState>(TriggerCommand<TState> trigger) where TState : Enum;
//        State<TState> GetState<TState>(Guid id) where TState : Enum;
//        void SetStateFactory(StateFactory stateFactory);
        event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException;
        void Start();
        void Stop();
        Task WaitAllHandlersProcessedAsync(int waitDelayMilliseconds = 100);
        void WaitAllHandlersProcessed(int waitDelayMilliseconds = 100);
    }
}