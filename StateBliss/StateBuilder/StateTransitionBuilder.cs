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

        internal StateTransitionBuilder(State state)
        {
            _state = state;
        }

//        public object Context { get; private set; }

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

//        public void OnEntered(TState state, OnStateEnterHandler<TState> handler)
//        {
//            AddHandler(-1, state.ToInt(), HandlerType.OnEntered, handler: handler);
//        }
//
//        public void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
//            where T : class
//        {
//            AddHandler(-1, state.ToInt(), HandlerType.OnEntered, handler.GetMethodName(), target: target);
//        }
//
//        public void OnEntered<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new()
//        {
//            AddHandlerWithContext(HandlerType.OnEnteredWithContext, -1,
//                _stateTransitionInfo.To, Context as TContext, handlers.Guards);
//        }
//
//        public void OnEntering<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddOnStateEnteringGuards(state.ToInt(), handlers.Context, handlers.Guards);
//        }
//
//        public void OnExited(TState state, OnStateExitHandler<TState> handler)
//        {
//            AddHandler(state.ToInt(), -1, HandlerType.OnExited, handler: handler);
//        }
//
//        public void OnExited<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler)
//            where T : class
//        {
//            AddHandler(state.ToInt(), -1, HandlerType.OnExited, handler.GetMethodName(), target: target);
//        }
//
//        public void OnExited<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new()
//        {
//            AddHandlerWithContext(HandlerType.OnExitedWithContext, _stateTransitionInfo.From,
//                -1, Context as TContext, handlers.Guards);
//        }
//
//        public void OnExiting<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddOnStateExitingGuards(state.ToInt(), handlers.Context, handlers.Guards);
//        }
//
//        public void OnEdited(TState state, OnStateEnterHandler<TState> handler)
//        {
//            var currentState = state.ToInt();
//            AddHandler(currentState, currentState, HandlerType.OnEdited, handler: handler);
//        }
//
//        public void OnEdited<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
//            where T : class
//        {
//            var current = state.ToInt();
//            AddHandler(current, current, HandlerType.OnEdited, handler.GetMethodName());
//        }
//
//        public void OnEdited<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : HandlerStateContext<TState>, new()
//        {
//            AddHandlerWithContext(HandlerType.OnEditedWithContext, _stateTransitionInfo.From,
//                -1, Context as TContext, handlers.Guards);
//        }
//
//        public void OnEditing<TContext>(TState state, IHandlersInfo<TContext> handlers)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddOnStateEditingGuards(state.ToInt(), handlers.Context, handlers.Guards);
//        }

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

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand>(
            OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>> handler)
            where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new GuardActionInfo<TState, TTriggerCommand>(null, handler, HandlerType.OnChanging));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing(
            OnStateEventGuardHandler<TState, TriggerCommand<TState>> handler)
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new GuardActionInfo<TState>(handler, HandlerType.OnChanging));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand>(
            params OnStateEventGuardHandler<TState, TTriggerCommand, GuardStateContext<TState>>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>
        {
            foreach (var handler in handlers)
                AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                    new GuardActionInfo<TState, TTriggerCommand, GuardStateContext<TState>>(null,
                        () => new GuardStateContext<TState>(), handler, HandlerType.OnChanging));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand, TContext>(Func<TContext> contextProvider,
            params OnStateEventGuardHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>, new()
            where TContext : GuardStateContext<TState>, new()
        {
            foreach (var handler in handlers)
                AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                    new GuardActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider, handler,
                        HandlerType.OnChanging));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T, TTriggerCommand>(T target,
            Expression<Func<T, OnStateEventGuardHandler<TState, TTriggerCommand>>> handler)
            where T : class where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new GuardActionInfo<TState, TTriggerCommand>(null, handler.GetMethodName(), HandlerType.OnChanging,
                    target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target,
            Expression<Func<T, OnStateEventGuardHandler<TState, TriggerCommand<TState>>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnChanging, target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T, TTriggerCommand, TContext>(T target,
            Func<TContext> contextProvider,
            params Expression<Func<T, OnStateEventGuardHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>, new()
            where TContext : GuardStateContext<TState>, new()
        {
            foreach (var handler in handlers)
                AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                    new GuardActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider,
                        handler.GetMethodName(),
                        HandlerType.OnChanging, target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<TTriggerCommand>(
            OnStateEventHandler<TState, TTriggerCommand> handler) where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                new ActionInfo<TState, TTriggerCommand>(null, handler, HandlerType.OnChanged));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed(OnStateEventHandler<TState, TriggerCommand<TState>> handler)
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                new ActionInfo<TState, TriggerCommand<TState>>(null, handler, HandlerType.OnChanged));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<TTriggerCommand, TContext>(Func<TContext> contextProvider,
            params OnStateEventHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>, new() where TContext : new()
        {
            foreach (var handler in handlers)
                AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                    new ActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider, handler,
                        HandlerType.OnChanged));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T, TTriggerCommand>(T target,
            Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand>>> handler) where T : class
            where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                new ActionInfo<TState, TTriggerCommand>(null, handler.GetMethodName(), HandlerType.OnChanged, target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T>(T target,
            Expression<Func<T, OnStateEventHandler<TState, TriggerCommand<TState>>>> handler)
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                new ActionInfo<TState, TriggerCommand<TState>>(null, handler.GetMethodName(), HandlerType.OnChanged,
                    target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T, TTriggerCommand, TContext>(T target,
            Func<TContext> contextProvider,
            params Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>, new() where TContext : new()
        {
            foreach (var handler in handlers)
                AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                    new ActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider, handler.GetMethodName(),
                        HandlerType.OnChanged, target));
            return this;
        }

        public IStateTransitionBuilder<TState> TriggeredBy(string trigger)
        {
            Triggers.Add((trigger, _stateTransitionInfo.From, _stateTransitionInfo.To, _state));
            return this;
        }

        internal ActionInfo[] GetOnTransitioningHandlers(int fromState, int toState)
        {
            return GetHandlers(HandlerType.OnChanging, fromState, toState);
        }

        internal ActionInfo[] GetOnEditHandlers(int currentState)
        {
            return GetHandlers(HandlerType.OnEditing, currentState, currentState);
        }

        internal ActionInfo[] GetOnEditGuardHandlers(int curentState)
        {
            return GetGuardHandlers(HandlerType.OnEditing, curentState, curentState);
        }

        internal ActionInfo[] GetOnChangedHandlers(int fromState, int toState)
        {
            return GetHandlers(HandlerType.OnChanged, fromState, toState);
        }

        internal ActionInfo[] GetOnExitHandlers(int fromState)
        {
            return GetHandlers(HandlerType.OnExiting, fromState, null);
        }

        internal ActionInfo[] GetOnEnterHandlers(int toState)
        {
            return GetHandlers(HandlerType.OnEntering, null, toState);
        }

        internal ActionInfo[] GetOnChangingGuardHandlers(int fromState, int toState)
        {
            return GetGuardHandlers(HandlerType.OnChanging, fromState, toState);
        }

        internal ActionInfo[] GetOnEnterGuardHandlers(int toState)
        {
            return GetGuardHandlers(HandlerType.OnEntering, null, toState);
        }

        internal ActionInfo[] GetOnExitGuardHandlers(int fromState)
        {
            return GetGuardHandlers(HandlerType.OnExiting, fromState, null);
        }

        internal ActionInfo[] GetOnTransitionedWithContextHandlers(int fromState, int toState)
        {
            return GetGuardHandlers(HandlerType.OnChanged, fromState, toState);
        }

        public TState[] GetNextStates(TState state)
        {
            var stateFilter = state.ToInt();
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => a.To.ToEnum<TState>()).ToArray();
        }

//        internal void AddOnStateEnteringGuards<TContext>(int state, TContext context,
//            IEnumerable<OnStateEventHandler<TContext>> guards)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddGuardHandler(HandlerType.OnEnteringGuard, -1, state, context, guards);
//        }
//
//        internal void AddOnStateExitingGuards<TContext>(int state, TContext context,
//            IEnumerable<OnStateEventHandler<TContext>> guards)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddGuardHandler(HandlerType.OnExitingGuard, state, -1, context, guards);
//        }
//
//        internal void AddOnStateEditingGuards<TContext>(int state, TContext context,
//            IEnumerable<OnStateEventHandler<TContext>> guards)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddGuardHandler(HandlerType.OnEditingGuard, state, state, context, guards);
//        }

//        internal void AddOnStateChangingGuards<TContext>(int fromState, int toState, TContext context,
//            IEnumerable<OnStateEventHandler<TContext>> guards)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddGuardHandler(HandlerType.OnChangingGuard, fromState, toState, context, guards);
//        }

        private void AddHandler(int fromState, int toState, HandlerType handlerType, ActionInfo actionInfo)
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

            stateTransitionInfo.Handlers.Add((actionInfo, handlerType));
        }


//        
//        private Delegate CreateActionInfo<TTriggerCommand>(OnStateEventHandler<TState, TTriggerCommand> handler)
//            where TContext : new()
//            where TTriggerCommand : TriggerCommand<TState>
//        {
//            
//        }

//        internal void AddGuardHandler<TContext>(HandlerType handlerType, int fromState, int toState, TContext context,
//            IEnumerable<OnStateEventHandler<TContext>> guards)
//            where TContext : GuardStateContext<TState>, new()
//        {
//            AddHandlerWithContext(handlerType, fromState, toState, context, guards);
//        }

        private void AddHandlerWithContext<TTriggerCommand>(HandlerType handlerType, int fromState, int toState,
            TTriggerCommand context,
            IEnumerable<OnStateEventHandler<TState, TTriggerCommand>> handlers)
            where TTriggerCommand : TriggerCommand<TState>
        {
            var stateType = typeof(TState);
            if (context != null && context.StateType != stateType)
                throw new StateTypeMismatchException(
                    $"The type {context.StateType.FullName} is not matched to the required type {stateType.FullName}");

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

            foreach (var handler in handlers)
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TTriggerCommand>(context, handler, handlerType),
                    handlerType));
        }

        public void SetContext(object context)
        {
            foreach (var stateTransitionInfo in _stateTransitions)
            foreach (var handlerInfo in stateTransitionInfo.Handlers)
                handlerInfo.Item1.Context = context;
        }

        public void SetCommand(TriggerCommand command)
        {
            foreach (var stateTransitionInfo in _stateTransitions)
            foreach (var handlerInfo in stateTransitionInfo.Handlers)
                handlerInfo.Item1.Command = command;
        }

        private ActionInfo[] GetGuardHandlers(HandlerType handlerType, int? fromState, int? toState)
        {
            return GetHandlers(handlerType, fromState, toState)
                .ToArray();
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