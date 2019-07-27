using System;

namespace StateBliss
{
    public interface IState<TState> where TState : Enum
    {
        TState Current { get; }
        TState[] GetNextStates();
        IStateMachineManager Manager { get; }
        object Entity { get; }
    }
}