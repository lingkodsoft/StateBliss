using System;

namespace StateBliss
{
    public abstract class ParentStateContext
    {
        protected ParentStateContext(Type stateType)
        {
            StateType = stateType;
        }

        public Type StateType { get; private set; }
        internal StateContext Context { get; set; }
    }
    
    public abstract class ParentStateContext<TState> : ParentStateContext
        where TState : Enum
    {
        internal new StateContext<TState> Context { get; set; }

        public ParentStateContext() : base(typeof(TState))
        {
        }
    }
}