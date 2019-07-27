using System;

namespace StateBliss
{
    public interface IStateMachineManager
    {
        void Register(State state);
        void ChamgeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum;
        event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException;
    }
}