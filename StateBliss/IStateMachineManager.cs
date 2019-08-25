using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StateBliss
{
    public interface IStateMachineManager : IDisposable
    {
        void Register(IEnumerable<Assembly> assemblyDefinitions, Func<Type, object> serviceProvider = null);
        StateChangeResult<TState, TData> Trigger<TState, TData>(TState currentState, TState nextState, TData context) where TState : Enum;
        TState[] GetNextStates<TState>(TState currentState) where TState : Enum;
        event EventHandler<(Exception exception,StateChangeInfo changeInfo)> OnHandlerException;
        void Start();
        void Stop();
        Task WaitAllHandlersProcessedAsync(int waitDelayMilliseconds = 100);
        void WaitAllHandlersProcessed(int waitDelayMilliseconds = 100);
    }
}