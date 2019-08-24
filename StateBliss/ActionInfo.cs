using System;

namespace StateBliss
{
    public abstract class ActionInfo
    {
        public abstract void Execute(StateChangeInfo changeInfo);
        protected TDelegate CreateDelegateFromInstance<TDelegate>(object target, string methodName)
            where TDelegate : Delegate
        {
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target, methodName);
        }
    }
    
    public class ActionInfo<TState> : ActionInfo
        where TState : Enum
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private StateChangeHandler<TState> _handler;

        public ActionInfo(StateChangeHandler<TState> handler, HandlerType handlerType)
        {
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public ActionInfo(string methodName, HandlerType handlerType, object target)
        {
            _methodName = methodName;
            _handlerType = handlerType;
            _handler = CreateDelegateFromInstance<StateChangeHandler<TState>>(target, _methodName);
        }

        public override void Execute(StateChangeInfo changeInfo)
        {
            if (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnEntered || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnChanged ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnExited ||
                _handlerType == HandlerType.OnEdited ||
                _handlerType == HandlerType.OnEditing
                )
            {
                _handler((StateChangeInfo<TState>)changeInfo);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}