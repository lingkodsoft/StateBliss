using System;

namespace StateBliss
{
    internal abstract class ActionInfo
    {
        public abstract void Execute(State state, int fromState, int toState);
        public abstract void SetObject(object target);
    }
    
    internal class ActionInfo<TState> : ActionInfo
        where TState : Enum
    {
        private Delegate _method;
        private readonly HandlerType _handlerType;
        private object _target;
        
        public ActionInfo(Delegate handler, HandlerType handlerType)
        {
            _method = handler;
            _handlerType = handlerType;
        }

        public ActionInfo(string methodName, HandlerType handlerType, object target)
        {
            _handlerType = handlerType;
            _target = target;
            CreateDelegate(methodName);
        }

        public override void SetObject(object target)
        {
            _target = target;
        }

        private void CreateDelegate(string methodName)
        {
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    CreateDelegateFromInstance<OnStateEnterHandler<TState>>(methodName);
                    break;
                case HandlerType.OnExit:
                    CreateDelegateFromInstance<OnStateExitHandler<TState>>(methodName);
                    break;
                case HandlerType.OnTransitioning:
                    CreateDelegateFromInstance<OnStateTransitioningHandler<TState>>(methodName);
                    break;
                case HandlerType.OnTransitioned:
                    CreateDelegateFromInstance<OnStateTransitionedHandler<TState>>(methodName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateDelegateFromInstance<TDelegate>(string methodName)
            where TDelegate : Delegate
        {
            _method = Delegate.CreateDelegate(typeof (TDelegate), _target, methodName);
        }

        public override void Execute(State state, int fromState, int toState)
        {
            var @from = (TState)(object)fromState;
            var @to = (TState)(object)toState;
            
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    ((OnStateEnterHandler<TState>)_method)(@to, (IState<TState>)state);
                    break;
                case HandlerType.OnExit:
                    ((OnStateExitHandler<TState>)_method)(@from, (IState<TState>)state);
                    break;
                case HandlerType.OnTransitioning:
                    ((OnStateTransitioningHandler<TState>)_method)((IState<TState>)state, @to);
                    break;
                case HandlerType.OnTransitioned:
                    ((OnStateTransitionedHandler<TState>)_method)(@from, (IState<TState>)state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}