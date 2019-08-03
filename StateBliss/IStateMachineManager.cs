using System;
using System.Threading.Tasks;

namespace StateBliss
{
    public interface IStateMachineManager : IDisposable
    {
        void Register(State state);
        bool ChangeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum;
        bool ChangeState<TState>(State<TState> state, TState newState) where TState : Enum;
        bool ChangeState<TState>(TState newState, Guid id) where TState : Enum;
        State<TState> GetState<TState>(Guid id) where TState : Enum;
        void SetStateFactory(StateFactory stateFactory);
        event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException;
        void Start();
        void Stop();
        Task WaitAllHandlersProcessedAsync(int waitDelayMilliseconds = 100);
        void WaitAllHandlersProcessed(int waitDelayMilliseconds = 100);
    }
}