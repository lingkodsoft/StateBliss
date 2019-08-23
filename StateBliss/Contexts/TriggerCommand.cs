using System;
using System.Collections.Generic;

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

        public IDictionary<string, object> Data => new Dictionary<string, object>(); 
        public bool ChangeStateSucceeded { get; internal set; }
    }
    
    public class TriggerCommand<TState> : TriggerCommand
        where TState : Enum
    {
        public TriggerCommand(TState nextState, Guid uid, Action<IDictionary<string, object>> setData = null) 
            : base(typeof(TState))
        {
            Uid = uid;
            NextState = nextState;
            setData?.Invoke(Data);
        }
        
        public TriggerCommand() : base(typeof(TState))
        {
        }

        public TState NextState { get; set; }

        public State State {get; internal set; }
    }
}