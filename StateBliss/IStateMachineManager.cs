using System;

namespace StateBliss
{
    public interface IStateMachineManager
    {
        void Register(State state);
        void ChamgeState<TStatus>(State state, TStatus newState) where TStatus : Enum;
    }
}