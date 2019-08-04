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

        public readonly List<TState> DisabledSameStateTransitioned = new List<TState>();
        private StateTransitionInfo _stateTransitionInfo;

        public StateTransitionBuilder(State state)
        {
            _state = state;
        }

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

        public void OnEntering<TContext>(TState state, IGuardsInfo<TContext> guards)
            where TContext : GuardContext<TState>
        {
            AddOnStateEnterGuards(state, guards.Context, guards.Guards);
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

        public void OnExiting<TContext>(TState state, IGuardsInfo<TContext> guards)
            where TContext : GuardContext<TState>
        {
            AddOnStateExitGuards(state, guards.Context, guards.Guards);
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

        public void OnEditing<TContext>(TState state, IGuardsInfo<TContext> guards)
            where TContext : GuardContext<TState>
        {
            AddOnStateEditGuards(state, guards.Context, guards.Guards);
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
                new ActionInfo<TState>(handler, HandlerType.OnTransitioned),
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T>(T target,
            Expression<Func<T, OnStateTransitionedHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioned, target),
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler)
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnTransitioning),
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target,
            Expression<Func<T, OnStateTransitioningHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioning, target),
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> TriggeredBy(string trigger)
        {
            Triggers.Add((trigger, _stateTransitionInfo.From, _stateTransitionInfo.To, _state));
            return this;
        }

        internal ActionInfo[] GetOnTransitioningHandlers(TState fromState, TState toState)
        {
            return GetHandlers(HandlerType.OnTransitioning, fromState.ToInt(), toState.ToInt());
        }

        internal ActionInfo[] GetOnEditHandlers(TState currentState)
        {
            return GetHandlers(HandlerType.OnEdit, currentState.ToInt(), currentState.ToInt());
        }

        public ActionInfo[] GetOnEditGuardHandlers(TState curentState)
        {
            return GetHandlers(HandlerType.OnEditGuard, curentState.ToInt(), curentState.ToInt());
        }

        internal ActionInfo[] GetOnTransitionedHandlers(TState fromState, TState toState)
        {
            return GetHandlers(HandlerType.OnTransitioned, fromState.ToInt(), toState.ToInt());
        }

        internal ActionInfo[] GetOnExitHandlers(TState fromState)
        {
            return GetHandlers(HandlerType.OnExit, fromState.ToInt(), null);
        }

        internal ActionInfo[] GetOnEnterHandlers(TState toState)
        {
            return GetHandlers(HandlerType.OnEnter, null, toState.ToInt());
        }

        public ActionInfo<TState>[] GetOnEnterGuardHandlers(TState toState)
        {
            return GetHandlers(HandlerType.OnEnterGuard, null, toState.ToInt())
                .Select(a => (ActionInfo<TState>) a).ToArray();
        }

        public ActionInfo<TState>[] GetOnExitGuardHandlers(TState fromState)
        {
            return GetHandlers(HandlerType.OnExitGuard, fromState.ToInt(), null)
                .Select(a => (ActionInfo<TState>) a).ToArray();
        }

        public TState[] GetNextStates(TState state)
        {
            var stateFilter = state.ToInt();
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => a.To.ToEnum<TState>()).ToArray();
        }

        internal void AddOnStateEnterGuards<TContext>(TState state, TContext context,
            IEnumerable<OnGuardHandler<TContext>> guards)
            where TContext : GuardContext
        {
            AddGuardHandler(HandlerType.OnEnterGuard, -1, state.ToInt(), context, guards);
        }

        internal void AddOnStateExitGuards<TContext>(TState state, TContext context,
            IEnumerable<OnGuardHandler<TContext>> guards)
            where TContext : GuardContext
        {
            AddGuardHandler(HandlerType.OnExitGuard, state.ToInt(), -1, context, guards);
        }

        internal void AddOnStateEditGuards<TContext>(TState state, TContext context,
            IEnumerable<OnGuardHandler<TContext>> guards)
            where TContext : GuardContext
        {
            var to = state.ToInt();
            AddGuardHandler(HandlerType.OnEnterGuard, to, to, context, guards);
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
                    ? new ActionInfo<TState>(handlerMethodName, handlerType, target)
                    : new ActionInfo<TState>(handler, handlerType), handlerType));
        }

        private void AddGuardHandler<TContext>(HandlerType handlerType, int fromState, int toState, TContext context,
            IEnumerable<OnGuardHandler<TContext>> guards)
            where TContext : GuardContext
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

        public void SetContext(object context)
        {
            foreach (var stateTransitionInfo in _stateTransitions)
            {
                foreach (var handlerInfo in stateTransitionInfo.Handlers)
                {
                    handlerInfo.Item1.SetContext(context);
                }
            }
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