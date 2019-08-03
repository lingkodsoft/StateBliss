using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateBliss
{
    internal class StateTransitionBuilder
    {
        protected State _state;
        internal readonly List<(string trigger, int? fromState, int toState, State state)> Triggers = new List<(string trigger, int? fromState, int toState, State state)>();
    }

    internal class StateTransitionBuilder<TState>: StateTransitionBuilder, IStateTransitionBuilder<TState>, IStateFromBuilder<TState>, IStateToBuilder<TState> 
        where TState : Enum
    {
        private readonly List<StateTransitionInfo> _stateTransitions = new List<StateTransitionInfo>();
        private StateTransitionInfo _stateTransitionInfo;
        
        public readonly List<TState> DisabledSameStateTransitioned = new List<TState>();

        public StateTransitionBuilder(State state)
        {
            _state = state;
        }
        
        public IStateToBuilder<TState> From(TState state)
        {
            _stateTransitionInfo =  new StateTransitionInfo
            {
                From = state.ToInt()
            };
            return this;
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = state.ToInt();
            _stateTransitions.Add(_stateTransitionInfo);
            return this;
        }

        public IStateTransitionBuilder<TState> Changed(OnStateTransitionedHandler<TState> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler, HandlerType.OnTransitioned), 
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioned, target), 
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing(OnStateTransitioningHandler<TState> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler, HandlerType.OnTransitioning), 
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioning, target), 
                HandlerType.OnTransitioning));
            return this;
        }

        public IStateTransitionBuilder<TState> TriggeredBy(string trigger)
        {
            Triggers.Add((trigger, _stateTransitionInfo.From, _stateTransitionInfo.To, _state));
            return this;
        }

        public void TriggerTo(TState nextState, string trigger)
        {
            Triggers.Add((trigger, null, nextState.ToInt(), _state));
        }

        public void OnEnter(TState state, OnStateEnterHandler<TState> handler)
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEnter, handler: handler);
        }

        public void OnEnter<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
        {
            var @to = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == -1 && a.To == @to);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = -1,
                    To = @to
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEnter, target), 
                HandlerType.OnEnter));
        }

        public void OnEnter<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>
        {
            AddOnStateEnterGuards(state, guards.Context, guards.Guards);
        }
        
        public void OnExit<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>
        {
            AddOnStateExitGuards(state, guards.Context, guards.Guards);
        }

        public void OnExit(TState state, OnStateExitHandler<TState> handler)
        {
            var @from = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From ==  @from && a.To == -1);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = @from,
                    To = -1
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnExit), 
                HandlerType.OnExit));
        }

        public void OnExit<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler)
        {
            var @from = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == @from && a.To == -1);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = @from,
                    To = -1
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExit, target), 
                HandlerType.OnExit));
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
                handler == null ?
                    new ActionInfo<TState>(handlerMethodName, handlerType, target):
                    new ActionInfo<TState>(handler, handlerType), handlerType));
        }
        
        public void OnEdit(TState state, OnStateEnterHandler<TState> handler)
        {
            var @to = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == -1 && a.To == @to);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = -1,
                    To = @to
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnEnter), 
                HandlerType.OnEnter));
        }

        public void OnEdit<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
        {
            var current = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == current && a.To == current);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = current,
                    To = current
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEdit, target), 
                HandlerType.OnEdit));
        }

        public void OnEdit<TContext>(TState state, GuardsInfo<TState, TContext> guards) where TContext : GuardContext<TState>
        {
            AddOnStateEditGuards(state, guards.Context, guards.Guards);
        }

        
        public void DisableSameStateTransitionFor(params TState[] states)
        {
            DisabledSameStateTransitioned.AddRange(states);
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
                .Select(a => (ActionInfo<TState>)a).ToArray();
        }
        
        public ActionInfo<TState>[] GetOnExitGuardHandlers(TState fromState)
        {
            return GetHandlers(HandlerType.OnExitGuard, fromState.ToInt(), null)
                .Select(a => (ActionInfo<TState>)a).ToArray();
        }
        
        private ActionInfo[] GetHandlers(HandlerType handlerType, int? fromState, int? toState)
        {
            var @to = toState ?? -1;
            var @from = fromState ?? -1;

            Func<StateTransitionInfo, bool> filter;

            if (fromState != null && toState != null)
            {
                filter = s => s.From == @from && s.To == @to;
            }
            else if (fromState != null)
            {
                filter = s => s.From == @from;
            }
            else
            {
                filter = s => s.To == @to;
            }
                
            return _stateTransitions
                .Where(filter)
                .SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == handlerType)
                .Select(a => a.Item1).ToArray();
        }
        
        public TState[] GetNextStates(TState state)
        {
            var stateFilter = state.ToInt();
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => a.To.ToEnum<TState>()).ToArray();
        }

        internal void AddOnStateEnterGuards<TContext>(TState state, TContext context, IEnumerable<OnGuardHandler<TState,TContext>> guards) 
            where TContext : GuardContext<TState>
        {
            var @to = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == -1 && a.To == @to);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = -1,
                    To = @to
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            foreach (var handler in guards)
            {
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TContext>(context, handler, HandlerType.OnEnterGuard), 
                    HandlerType.OnEnterGuard));
            }
        }
        
        internal void AddOnStateExitGuards<TContext>(TState state, TContext context, IEnumerable<OnGuardHandler<TState,TContext>> guards) 
            where TContext : GuardContext<TState>
        {
            var @from = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == @from && a.To == -1);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = @from,
                    To = -1
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            foreach (var handler in guards)
            {
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TContext>(context, handler, HandlerType.OnExitGuard), 
                    HandlerType.OnExitGuard));
            }
        }

        internal void AddOnStateEditGuards<TContext>(TState state, TContext context, IEnumerable<OnGuardHandler<TState,TContext>> guards) 
            where TContext : GuardContext<TState>
        {
            var @to = state.ToInt();
            var stateTransitionInfo = _stateTransitions.SingleOrDefault(a => a.From == @to && a.To == @to);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = @to,
                    To = @to
                };
                _stateTransitions.Add(stateTransitionInfo);
            }
            foreach (var handler in guards)
            {
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TContext>(context, handler, HandlerType.OnEditGuard), 
                    HandlerType.OnEditGuard));
            }
        }
    }
}