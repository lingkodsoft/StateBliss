using System;

namespace StateBliss
{
    public class HandlerStateContext<TState, TParentContext> : StateContext<TState>
        where TParentContext : ParentStateContext<TState>
        where TState : Enum
    {
        public new TParentContext ParentContext
        {
            get => (TParentContext)base.ParentContext;
            internal set => base.ParentContext = value;
        }
    }
}