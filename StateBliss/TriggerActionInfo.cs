using System;

namespace StateBliss
{
    internal class TriggerActionInfo<TState, TContext> : ActionInfo
        where TState : Enum
        where TContext : ParentStateContext
    {
        private Delegate _method;
        private readonly HandlerType _handlerType;
        private object _target;

        public TriggerActionInfo(Delegate handler, HandlerType handlerType, TContext context)
            :base(context, true)
        {
            Context = context;
            _method = handler;
            _handlerType = handlerType;
        }

        public TriggerActionInfo(string methodName, HandlerType handlerType, object target, TContext context)
            :base(context, true)
        {
            Context = context;
            _handlerType = handlerType;
            _target = target;
            CreateDelegate(methodName);
        }

        public override void SetTarget(object target)
        {
            _target = target;
        }
        
        private void CreateDelegate(string methodName)
        {
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    _method = CreateDelegateFromInstance(typeof(OnStateEnterHandler<TState, TContext>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnExit:
                    _method = CreateDelegateFromInstance(typeof(OnStateExitHandler<TState, TContext>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnTransitioning:
                    _method = CreateDelegateFromInstance(typeof(OnStateTransitioningHandler<TState, TContext>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnTransitioned:
                    _method = CreateDelegateFromInstance(typeof(OnStateTransitionedHandler<TState, TContext>), _target, methodName, _handlerType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Delegate CreateDelegateFromInstance(Type type, object target, string methodName, HandlerType handlerType)
        {
            return Delegate.CreateDelegate(type, target, methodName);
        }

        public override void Execute(State state, int fromState, int toState)
        {
            if (!(Context is TContext context))
            {
                return;
            }
            
            var @from = fromState.ToEnum<TState>();
            var @to = toState.ToEnum<TState>();
            
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    ((OnStateEnterHandler<TState, TContext>) _method)(@to, (IState<TState>) state, context);
                    break;
                case HandlerType.OnExit:
                    ((OnStateExitHandler<TState, TContext>)_method)(@from, (IState<TState>)state, context);
                    break;
                case HandlerType.OnEdit:
                    ((OnStateEnterHandler<TState, TContext>)_method)(@to, (IState<TState>)state, context);
                    break;
                case HandlerType.OnTransitioning:
                    ((OnStateTransitioningHandler<TState, TContext>)_method)((IState<TState>)state, @to, context);
                    break;
                case HandlerType.OnTransitioned:
                    ((OnStateTransitionedHandler<TState, TContext>)_method)(@from, (IState<TState>)state, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}