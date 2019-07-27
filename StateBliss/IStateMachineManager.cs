using System;

namespace StateBliss
{
    public interface IStateMachineManager
    {
        void Register(State state);
        void ChamgeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum;
    }
}