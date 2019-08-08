using System;

namespace StateBliss
{
    public interface IStateToBuilder<TState> where TState : Enum
    {
        IStateTransitionBuilder<TState> To(TState state);
    }
}