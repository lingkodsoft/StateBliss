using System;

namespace StateBliss
{
    public abstract class TriggerCommand
    {
        protected TriggerCommand(Type stateType)
        {
            StateType = stateType;
        }

        public Type StateType { get; private set; }
        
        public Guid Uid { get; set; }

        public bool ChangeStateSucceeded { get; internal set; }
    }
    
    public abstract class TriggerCommand<TState> : TriggerCommand
        where TState : Enum
    {
        public TriggerCommand() : base(typeof(TState))
        {
        }

        public TState NextState { get; set; }

        public IState<TState> State {get; internal set; }
    }
}