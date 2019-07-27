using System;
using System.Threading.Tasks;

namespace StateBliss
{
    public interface IStateMachineManager
    {
        void Register(State state);
        void ChangeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum;
        event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException;
        void Start();
        void Stop();
        Task WaitAllHandlersProcessed(int waitDelayMilliseconds = 100);
    }
}