using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateBliss
{
    internal class StateTransitionBuilder
    {
        internal readonly List<(string trigger, int? fromState, int toState, State state)> Triggers =
            new List<(string trigger, int? fromState, int toState, State state)>();

        protected State _state;
    }

    internal class StateTransitionBuilder<TState> : StateTransitionBuilder, IStateTransitionBuilder<TState>,
        IStateFromBuilder<TState>, IStateToBuilder<TState>
        where TState : Enum
    {
        private readonly List<StateTransitionInfo> _stateTransitions = new List<StateTransitionInfo>();
        private object _context;

        public readonly List<TState> DisabledSameStateTransitioned = new List<TState>();
        private StateTransitionInfo _stateTransitionInfo;

        internal StateTransitionBuilder(State state)
        {
            _state = state;
        }

        public object Context => _context;

        public IStateToBuilder<TState> From(TState state)
        {
            _stateTransitionInfo = new StateTransitionInfo
            {
                From = state.ToInt()
            };
            return this;
        }

        public void TriggerTo(TState nextState, string trigger)
        {
            Triggers.Add((trigger, null, nextState.ToInt(), _state));
        }

        public void OnEntered(TState state, OnStateEnterHandler<TState> handler)
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEnter, handler: handler);
        }

        public void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
            where T: class
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEnter, handler.GetMethodName(), target: target);
        }

        public void OnEntering<TContext>(TState state, IHandlersInfo<TContext> handlers)
            where TContext : GuardStateContext<TState>, new()
        {
            AddOnStateEnterGuards(state.ToInt(), handlers.Context, handlers.Guards);
        }

        public void OnExited(TState state, OnStateExitHandler<TState> handler)
        {
            AddHandler(state.ToInt(), -1, HandlerType.OnExit, handler: handler);
        }

        public void OnExited<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler)
            where T: class
        {
            AddHandler(state.ToInt(), -1, HandlerType.OnExit, handler.GetMethodName(), target: target);
        }

        public void OnExiting<TContext>(TState state, IHandlersInfo<TContext> handlers)
            where TContext : GuardStateContext<TState>, new()
        {
            AddOnStateExitGuards(state.ToInt(), handlers.Context, handlers.Guards);
        }

        public void OnEdited(TState state, OnStateEnterHandler<TState> handler)
        {
            var currentState = state.ToInt();
            AddHandler(currentState, currentState, HandlerType.OnEdit, handler: handler);
        }

        public void OnEdited<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
            where T: class
        {
            var current = state.ToInt();
            AddHandler(current, current, HandlerType.OnEdit, handler.GetMethodName());
        }

        public void OnEditing<TContext>(TState state, IHandlersInfo<TContext> handlers)
            where TContext : GuardStateContext<TState>, new()
        {
            AddOnStateEditGuards(state.ToInt(), handlers.Context, handlers.Guards);
        }

        public void DisableSameStateTransitionFor(params TState[] states)
        {
            DisabledSameStateTransitioned.AddRange(states);
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = state.ToInt();
            _stateTransitions.Add(_stateTransitionInfo);
            return this;
        }

        public IStateTransitionBuilder<TState> Changed(OnStateTransitionedHandler<TState> handler)
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnTransitioned, new StateContext<TState>(), false),
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<TContext>(IHandlersInfo<TContext> handlers) 
            where TContext : HandlerStateContext, new()
        {          
            AddStateChangeHandlerWithContext(HandlerType.OnTransitionedWithContext, _stateTransitionInfo.From, _stateTransitionInfo.To, _context as TContext, handlers.Guards);
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T>(T target,
            Expression<Func<T, OnStateTransitionedHandler<TState>>> handler) where T : class
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioned, target, new StateContext<TState>(), false),
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler)
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnTransitioning, new StateContext<TState>(), false),
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target,
            Expression<Func<T, OnStateTransitioningHandler<TState>>> handler) where T : class
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioning, target, new StateContext<TState>(), false),
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<TContext>(IHandlersInfo<TContext> handlers) 
            where TContext : GuardStateContext<TState>, new()
        {
            AddOnStateChangingGuards(_stateTransitionInfo.From, _stateTransitionInfo.To, handlers.Context, handlers.Guards);
            return this;
        }

        public IStateTransitionBuilder<TState> TriggeredBy(string trigger)
        {
            Triggers.Add((trigger, _stateTransitionInfo.From, _stateTransitionInfo.To, _state));
            return this;
        }

        internal ActionInfo[] GetOnTransitioningHandlers(int fromState, int toState)
        {
            return GetHandlers(HandlerType.OnTransitioning, fromState, toState);
        }

        internal ActionInfo[] GetOnEditHandlers(int currentState)
        {
            return GetHandlers(HandlerType.OnEdit, currentState, currentState);
        }

        internal ActionInfo<TState>[] GetOnEditGuardHandlers(int curentState)
        {
            return GetGuardHandlers(HandlerType.OnEditGuard, curentState, curentState);
        }

        internal ActionInfo[] GetOnTransitionedHandlers(int fromState, int toState)
        {
            return GetHandlers(HandlerType.OnTransitioned, fromState, toState);
        }

        internal ActionInfo[] GetOnExitHandlers(int fromState)
        {
            return GetHandlers(HandlerType.OnExit, fromState, null);
        }

        internal ActionInfo[] GetOnEnterHandlers(int toState)
        {
            return GetHandlers(HandlerType.OnEnter, null, toState);
        }
        
        internal ActionInfo<TState>[] GetOnChangingGuardHandlers(int fromState, int toState)
        {
            return GetGuardHandlers(HandlerType.OnTransitioningGuard, fromState, toState);
        }
        
        internal ActionInfo<TState>[] GetOnEnterGuardHandlers(int toState)
        {
            return GetGuardHandlers(HandlerType.OnEnterGuard, null, toState);
        }

        internal ActionInfo<TState>[] GetOnExitGuardHandlers(int fromState)
        {
            return GetGuardHandlers(HandlerType.OnExitGuard, fromState, null);
        }
        
        internal ActionInfo<TState>[] GetOnTransitionedWithContextHandlers(int fromState, int toState)
        {
            return GetGuardHandlers(HandlerType.OnTransitionedWithContext, fromState, toState);
        }

        public TState[] GetNextStates(TState state)
        {
            var stateFilter = state.ToInt();
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => a.To.ToEnum<TState>()).ToArray();
        }

        internal void AddOnStateEnterGuards<TContext>(int state, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : GuardStateContext<TState>, new()
        {
            AddGuardHandler(HandlerType.OnEnterGuard, -1, state, context, guards);
        }

        internal void AddOnStateExitGuards<TContext>(int state, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : GuardStateContext<TState>, new()
        {
            AddGuardHandler(HandlerType.OnExitGuard, state, -1, context, guards);
        }

        internal void AddOnStateEditGuards<TContext>(int state, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : GuardStateContext<TState>, new()
        {
            AddGuardHandler(HandlerType.OnEnterGuard, state, state, context, guards);
        }
        
        internal void AddOnStateChangingGuards<TContext>(int fromState, int toState, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : GuardStateContext<TState>, new()
        {
            AddGuardHandler(HandlerType.OnTransitioningGuard, fromState, toState, context, guards);
        }

        private void AddHandler(int fromState, int toState, HandlerType handlerType,
            string handlerMethodName = null, Delegate handler = null, object target = null)
        {
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == fromState && a.To == toState);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = fromState,
                    To = toState
                };
                _stateTransitions.Add(stateTransitionInfo);
            }

            stateTransitionInfo.Handlers.Add((
                handler == null
                    ? new ActionInfo<TState>(handlerMethodName, handlerType, target, new StateContext<TState>(), false)
                    : new ActionInfo<TState>(handler, handlerType, new StateContext<TState>(), false), handlerType));
        }

        private void AddGuardHandler<TContext>(HandlerType handlerType, int fromState, int toState, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : GuardStateContext<TState>, new()
        {
            AddHandlerWithContext(handlerType, fromState, toState, context, guards);
        }
        
        private void AddStateChangeHandlerWithContext<TContext>(HandlerType handlerType, int fromState, int toState, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : StateContext, new()
        {
            AddHandlerWithContext(handlerType, fromState, toState, context, guards);
        }
        
        private void AddHandlerWithContext<TContext>(HandlerType handlerType, int fromState, int toState, TContext context,
            IEnumerable<OnStateEventHandler<TContext>> guards)
            where TContext : StateContext, new()
        {
            var stateType = typeof(TState);
            if (context != null && context.StateType != stateType)
            {
                throw new StateTypeMismatchException($"The type {context.StateType.FullName} is not matched to the required type {stateType.FullName}");
            }
            
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == fromState && a.To == toState);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = fromState,
                    To = toState
                };
                _stateTransitions.Add(stateTransitionInfo);
            }

            foreach (var handler in guards)
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TContext>(context, handler, handlerType),
                    handlerType));
        }

        public void SetContext(StateContext context)
        {
            _context = context;
            foreach (var stateTransitionInfo in _stateTransitions)
            {
                foreach (var handlerInfo in stateTransitionInfo.Handlers)
                {
                    handlerInfo.Item1.Context.ParentContext = context.ParentContext;
                }
            }
        }
        
        private ActionInfo<TState>[] GetGuardHandlers(HandlerType handlerType, int? fromState, int? toState)
        {
            return GetHandlers(handlerType, fromState, toState)
                .Select(a => (ActionInfo<TState>) a).ToArray();
        }
        
        private ActionInfo[] GetHandlers(HandlerType handlerType, int? fromState, int? toState)
        {
            var to = toState ?? -1;
            var from = fromState ?? -1;

            Func<StateTransitionInfo, bool> filter;

            if (fromState != null && toState != null)
                filter = s => s.From == from && s.To == to;
            else if (fromState != null)
                filter = s => s.From == from;
            else
                filter = s => s.To == to;

            return _stateTransitions
                .Where(filter)
                .SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == handlerType)
                .Select(a => a.Item1).ToArray();
        }
    }
}