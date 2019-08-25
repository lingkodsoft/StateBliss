using System;

namespace StateBliss
{
    public class StateChangeInfo<TState> : StateChangeInfo
        where TState : Enum
    {
        public new TState FromState
        {
            get => base.FromState.ToEnum<TState>();
            set => base.FromState = value.ToInt();
        }

        public new TState ToState
        {
            get => base.ToState.ToEnum<TState>();
            set => base.ToState = value.ToInt();
        }

        public TState CurrentState
        {
            get => base.CurrentState.ToEnum<TState>();
            set => base.CurrentState = value.ToInt();
        }

        internal StateChangeResult<TState, TData> ToResult<TData>()
        {
            return new StateChangeResult<TState, TData>
            {
                FromState = this.FromState,
                ToState = this.ToState,
                CurrentState = this.CurrentState,
                StateChangedSucceeded = this.StateChangedSucceeded
            };
        }
    }

    public class StateChangeInfo
    {
        internal object Data { get; set; }
        public T DataAs<T>() => (T)Data;
        internal int FromState { get; set; }
        internal int ToState { get; set; }
        internal int CurrentState { get; set; }
        public bool StateChangedSucceeded { get; internal set; }
    }
}