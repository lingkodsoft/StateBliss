using System;

namespace StateBliss
{
    public static class StateExtensions
    {
        public static State<TState> AsState<TState>(this TState state, Action<IStateFromBuilder<TState>> builderAction, 
            string name = null, bool registerToDefaultStateMachineManager = true)
            where TState : Enum
        {
            return new State<TState>(state, name, registerToDefaultStateMachineManager)
                .Define(builderAction);
        }
    }
}