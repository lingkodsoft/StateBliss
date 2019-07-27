using System;

namespace StateBliss
{
    internal abstract class ActionInfo
    {
        public abstract void Execute(State state, int fromState, int toState);
    }
    
    internal class ActionInfo<TState> : ActionInfo
        where TState : Enum
    {
        private Delegate _method;
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private readonly object _target;
        
        public ActionInfo(string methodName, HandlerType handlerType, object target)
        {
            _methodName = methodName;
            _handlerType = handlerType;
            _target = target;
            CreateDelegate();
        }

        private void CreateDelegate()
        {
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    CreateDelegateFromInstance<OnStateEnterHandler<TState>>();
                    break;
                case HandlerType.OnExit:
                    CreateDelegateFromInstance<OnStateExitHandler<TState>>();
                    break;
                case HandlerType.OnTransitioning:
                    CreateDelegateFromInstance<OnStateTransitioningHandler<TState>>();
                    break;
                case HandlerType.OnTransitioned:
                    CreateDelegateFromInstance<OnStateTransitionedHandler<TState>>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateDelegateFromInstance<TDelegate>()
            where TDelegate : Delegate
        {
            _method = Delegate.CreateDelegate(typeof (TDelegate), _target, _methodName);
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
                    CreateDelegateFromInstance<OnStateTransitionedHandler<TState>>();
                    ((OnStateTransitionedHandler<TState>)_method)(@from, (IState<TState>)state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}