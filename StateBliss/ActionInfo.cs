using System;

namespace StateBliss
{
    public abstract class ActionInfo
    {
        protected ActionInfo(TriggerCommand command, bool isTriggerAction)
        {
            Command = command;
            IsTriggerAction = isTriggerAction;
        }
        
        internal bool IsTriggerAction { get; private set; }
        
        public abstract void Execute(State state, int fromState, int toState);
        public abstract void SetTarget(object target);

        internal abstract object Context { get; set; }

        internal TriggerCommand Command { get; set; }
        
        internal StateContext StateContext => Context as StateContext;
        
        protected TDelegate CreateDelegateFromInstance<TDelegate>(object target, string methodName)
            where TDelegate : Delegate
        {
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target, methodName);
        }
    }
    
    public class ActionInfo<TContext> : ActionInfo
        where TContext : new()
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventHandler<TContext> _handler;
        private Lazy<TContext> _context;


        public ActionInfo(Func<TContext> contextProvider, OnStateEventHandler<TContext> handler, HandlerType handlerType) 
            : base(null, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public ActionInfo(Func<TContext> contextProvider, string methodName, HandlerType handlerType, object target) 
            : base(null, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }

        internal override object Context
        {
            get => _context.Value;
            set => _context = new Lazy<TContext>(() => (TContext)value);
        }

        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventHandler<TContext>>(target, _methodName);
        }

        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing
                ))
            {
                StateContext.FromState = fromState;
                StateContext.ToState = toState;
                StateContext.State = state;
                _handler((TContext)Context);
            }
            else if (
                _handlerType == HandlerType.OnEntering ||
                _handlerType == HandlerType.OnChanged ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnEdited ||
                _handlerType == HandlerType.OnEditing ||
                _handlerType == HandlerType.OnEntered ||
                _handlerType == HandlerType.OnExited)
            {
                StateContext.WhenSome(c => {
                    c.FromState = fromState;
                    c.ToState = toState;
                    c.State = state;        
                });
                
                _handler((TContext)Context);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
    
    internal class ActionInfo<TState, TTriggerCommand> : ActionInfo
        where TState : Enum
        where TTriggerCommand : TriggerCommand<TState>
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventHandler<TState, TTriggerCommand> _handler;

        public ActionInfo(TTriggerCommand command, OnStateEventHandler<TState, TTriggerCommand> handler, HandlerType handlerType) 
            : base(command, false)
        {
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public ActionInfo(TTriggerCommand command, string methodName, HandlerType handlerType, object target) 
            : base(command, false)
        {
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }

        internal override object Context { get; set; }

        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventHandler<TState, TTriggerCommand>>(target, _methodName);
        }
        
        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing
                ))
            {
                if (Command is TTriggerCommand cmd)
                {
                    StateContext.FromState = fromState;
                    StateContext.ToState = toState;
                    StateContext.State = state;
                    _handler(cmd);
                }
            }
            else if (
                _handlerType == HandlerType.OnEntering ||
                _handlerType == HandlerType.OnChanged ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnEdited ||
                _handlerType == HandlerType.OnEditing ||
                _handlerType == HandlerType.OnEntered ||
                _handlerType == HandlerType.OnExited)
            {
                if (Command is TTriggerCommand cmd)
                    _handler(cmd);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
    
    internal class ActionInfo<TState, TTriggerCommand, TContext> : ActionInfo
        where TState : Enum
        where TContext : new()
        where TTriggerCommand : TriggerCommand<TState>, new()
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventHandler<TState, TTriggerCommand, TContext> _handler;
        private Lazy<TContext> _context;

        public ActionInfo(TTriggerCommand command, Func<TContext> contextProvider, OnStateEventHandler<TState, TTriggerCommand, TContext> handler, HandlerType handlerType) 
            : base(command, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public ActionInfo(TTriggerCommand command, Func<TContext> contextProvider, string methodName, HandlerType handlerType, object target) 
            : base(command, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }
        
        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventHandler<TState, TTriggerCommand, TContext>>(target, _methodName);
        }

        internal override object Context
        {
            get => _context.Value;
            set => _context = new Lazy<TContext>(() => (TContext)value);
        }

        public new TTriggerCommand Command => base.Command as TTriggerCommand;

        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing))
            {
                if (Command is TTriggerCommand cmd)
                {
                    StateContext.FromState = fromState;
                    StateContext.ToState = toState;
                    StateContext.State = state;
                    _handler(cmd, (TContext) Context);
                }
            }
            else if (
                _handlerType == HandlerType.OnEntering ||
                _handlerType == HandlerType.OnChanged ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnEdited ||
                _handlerType == HandlerType.OnEditing ||
                _handlerType == HandlerType.OnEntered ||
                _handlerType == HandlerType.OnExited)
            {
                if (Command is TTriggerCommand cmd)
                {
                    StateContext.WhenSome(c => {
                        c.FromState = fromState;
                        c.ToState = toState;
                        c.State = state;        
                    });
                    _handler(cmd, (TContext)Context);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
    
    
    public class GuardActionInfo<TState, TTriggerCommand, TContext> : ActionInfo
        where TState : Enum
        where TContext : GuardStateContext<TState>, new()
        where TTriggerCommand : TriggerCommand<TState>
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventGuardHandler<TState, TTriggerCommand, TContext> _handler;
        private Lazy<TContext> _context;

        public GuardActionInfo(TTriggerCommand command, Func<TContext> contextProvider, OnStateEventGuardHandler<TState, TTriggerCommand, TContext> handler, HandlerType handlerType) 
            : base(command, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public GuardActionInfo(TTriggerCommand command, Func<TContext> contextProvider, string methodName, HandlerType handlerType, object target) 
            : base(command, false)
        {
            _context = new Lazy<TContext>(contextProvider);
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }
        
        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventGuardHandler<TState, TTriggerCommand, TContext>>(target, _methodName);
        }

        internal override object Context
        {
            get => _context.Value;
            set => _context = new Lazy<TContext>(() => (TContext)value);
        }

        internal new TContext StateContext => Context as TContext;
        
        public new TTriggerCommand Command => base.Command as TTriggerCommand;

        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing))
            {
                if (Command is TTriggerCommand cmd)
                {
                    StateContext.FromState = fromState.ToEnum<TState>();
                    StateContext.ToState = toState.ToEnum<TState>();
                    StateContext.State = state;
                    _handler(cmd, StateContext);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
    
    public class GuardActionInfo<TState, TTriggerCommand> : ActionInfo
        where TState : Enum
        where TTriggerCommand : TriggerCommand<TState>
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>> _handler;
        private Lazy<GuardStateContext<TState>> _context;

        public GuardActionInfo(TTriggerCommand command, OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>> handler, HandlerType handlerType) 
            : base(command, false)
        {
            _context = new Lazy<GuardStateContext<TState>>(() => new GuardStateContext<TState>());
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public GuardActionInfo(TTriggerCommand command, string methodName, HandlerType handlerType, object target) 
            : base(command, false)
        {
            _context = new Lazy<GuardStateContext<TState>>(() => new GuardStateContext<TState>());
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }
        
        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>>>(target, _methodName);
        }

        internal override object Context
        {
            get => _context.Value;
            set => _context = new Lazy<GuardStateContext<TState>>(() => (GuardStateContext<TState>)value);
        }

        internal new GuardStateContext<TState> StateContext => Context as GuardStateContext<TState>;
        
        public new TTriggerCommand Command => base.Command as TTriggerCommand;

        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing))
            {

                if (Command is TTriggerCommand cmd)
                {
                    StateContext.FromState = fromState.ToEnum<TState>();
                    StateContext.ToState = toState.ToEnum<TState>();
                    StateContext.State = state;
                    _handler(cmd, StateContext);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
    
    public class GuardActionInfo<TState> : ActionInfo
        where TState : Enum
    {
        private readonly string _methodName;
        private readonly HandlerType _handlerType;
        private OnStateEventGuardHandler<TState, TriggerCommand<TState>> _handler;
        private Lazy<GuardStateContext<TState>> _context;

        public GuardActionInfo(OnStateEventGuardHandler<TState, TriggerCommand<TState>> handler, HandlerType handlerType) 
            : base(null, false)
        {
            _context = new Lazy<GuardStateContext<TState>>(() => new GuardStateContext<TState>());
            _handler = handler;
            _handlerType = handlerType;
        }
        
        public GuardActionInfo(string methodName, HandlerType handlerType, object target) 
            : base(null, false)
        {
            _context = new Lazy<GuardStateContext<TState>>(() => new GuardStateContext<TState>());
            _methodName = methodName;
            _handlerType = handlerType;
            SetTargetInternal(target);
        }

        public override void SetTarget(object target)
        {
            SetTargetInternal(target);
        }
        
        private void SetTargetInternal(object target)
        {
            _handler = CreateDelegateFromInstance<OnStateEventGuardHandler<TState, TriggerCommand<TState>>>(target, _methodName);
        }

        internal override object Context
        {
            get => _context.Value;
            set => _context = new Lazy<GuardStateContext<TState>>(() => (GuardStateContext<TState>)value);
        }

        internal new GuardStateContext<TState> StateContext => Context as GuardStateContext<TState>;
        
        public override void Execute(State state, int fromState, int toState)
        {
            if (StateContext != null &&  (
                _handlerType == HandlerType.OnEntering || 
                _handlerType == HandlerType.OnChanging ||
                _handlerType == HandlerType.OnExiting ||
                _handlerType == HandlerType.OnEditing))
            {
                StateContext.FromState = fromState.ToEnum<TState>();
                StateContext.ToState = toState.ToEnum<TState>();
                StateContext.State = state;
                _handler((TriggerCommand<TState>)Command, StateContext);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}