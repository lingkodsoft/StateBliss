using System;

namespace StateBliss
{
    internal abstract class ActionInfo
    {
        public abstract void Execute(State state, int fromState, int toState);
        public abstract void SetObject(object target);
    }

    internal class ActionInfo<TState, TContext> : ActionInfo<TState>
        where TState : Enum
        where TContext : GuardContext<TState>
    {
        private readonly TContext _context;

        public ActionInfo(TContext context, Delegate handler, HandlerType handlerType) : base(handler, handlerType)
        {
            _context = context;
        }

        public override GuardContext<TState> GuardContext => _context;
        
        public ActionInfo(TContext context, string methodName, HandlerType handlerType, object target) : base(methodName, handlerType, target)
        {
            _context = context;
        }
        
        public override void Execute(State state, int fromState, int toState)
        {
            if (_handlerType == HandlerType.OnEnterGuard || 
                _handlerType == HandlerType.OnExitGuard ||
                _handlerType == HandlerType.OnEditGuard)
            {
                _context.FromState = toState.ToEnum<TState>();
                _context.ToState = toState.ToEnum<TState>();
                _context.State = (IState<TState>)state;
                ((OnGuardHandler<TState, TContext>)_method)(_context);
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
        
        public virtual GuardContext<TState> GuardContext { get; }

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
                    _method = CreateDelegateFromInstance(typeof(OnStateEnterHandler<TState>), _target, methodName, _handlerType);
                    break;
                case HandlerType.OnEnterGuard:
                    _method = CreateDelegateFromInstance(null, _target, methodName, _handlerType);
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