using System;

namespace StateBliss
{
    internal abstract class ActionInfo
    {
        protected ActionInfo(object context, bool isTriggerAction)
        {
            Context = context;
            IsTriggerAction = isTriggerAction;
        }
        
        internal bool IsTriggerAction { get; private set; }
        
        public abstract void Execute(State state, int fromState, int toState);
        public abstract void SetTarget(object target);
        internal object Context { get; set; }

    }

    internal class ActionInfo<TState, TContext> : ActionInfo<TState>
        where TState : Enum
        where TContext : StateContext, new()
    {

        public ActionInfo(TContext context, Delegate handler, HandlerType handlerType) 
            : base(handler, handlerType, context, false)
        {
            base.Context = context ?? new TContext();
        }
        
        public ActionInfo(TContext context, string methodName, HandlerType handlerType, object target) 
            : base(methodName, handlerType, target, context, false)
        {
            base.Context = context ?? new TContext();
        }
        
        public new TContext Context => (TContext) (base.Context);

        public override void Execute(State state, int fromState, int toState)
        {
            if (_handlerType == HandlerType.OnEnterGuard || 
                _handlerType == HandlerType.OnTransitioningGuard ||
                _handlerType == HandlerType.OnExitGuard ||
                _handlerType == HandlerType.OnEditGuard)
            {
                Context.FromState = fromState;
                Context.ToState = toState;
                Context.State = state;
                ((OnGuardHandler<TContext>)_method)(Context);
            }
            else
            {
                base.Execute(state, fromState, toState);    
            }
        }
    }
    
    internal class ActionInfo<TState> : ActionInfo
        where TState : Enum
    {
        protected Delegate _method;
        protected readonly HandlerType _handlerType;
        protected object _target;

        public ActionInfo(Delegate handler, HandlerType handlerType, object context, bool isTriggerAction)
            :base(context, isTriggerAction)
        {
            Context = context;
            _method = handler;
            _handlerType = handlerType;
        }

        public ActionInfo(string methodName, HandlerType handlerType, object target, object context, bool isTriggerAction)
            :base(context, isTriggerAction)
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
                    _method = CreateDelegateFromInstance(typeof(OnStateEnterHandler<TState>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnExit:
                    _method = CreateDelegateFromInstance(typeof(OnStateExitHandler<TState>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnTransitioning:
                    _method = CreateDelegateFromInstance(typeof(OnStateTransitioningHandler<TState>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnTransitioned:
                    _method = CreateDelegateFromInstance(typeof(OnStateTransitionedHandler<TState>), _target, methodName, _handlerType);
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
            var @from = fromState.ToEnum<TState>();
            var @to = toState.ToEnum<TState>();
            
            switch (_handlerType)
            {
                case HandlerType.OnEnter:
                    ((OnStateEnterHandler<TState>)_method)(@to, (IState<TState>)state);
                    break;
                case HandlerType.OnExit:
                    ((OnStateExitHandler<TState>)_method)(@from, (IState<TState>)state);
                    break;
                case HandlerType.OnEdit:
                    ((OnStateEnterHandler<TState>)_method)(@to, (IState<TState>)state);
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