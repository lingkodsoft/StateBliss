//using System;
//
//namespace StateBliss
//{
//    public class StateChangeTrigger<TState> : TriggerCommand<TState>
//        where TState : Enum
//    {
//        public StateChangeTrigger()
//        {
//            Context = new StateContext<TState> {ParentContext = this};
//        }
//
//        public Guid Uid { get => Context.Uid; set => Context.Uid = value; }
//
//        public bool ChangeStateSucceeded
//        {
//            get => Context.ChangeStateSucceeded;
//            internal set => Context.ChangeStateSucceeded = value;
//        }
//
//        public TState NextState
//        {
//            get => Context.ToState;
//            set => Context.ToState = value;
//        }
//
//        public IState<TState> State => Context.State;
//    }
//}