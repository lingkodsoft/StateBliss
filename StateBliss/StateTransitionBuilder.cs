using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateBliss
{
    internal class StateTransitionBuilder<TState>: IStateTransitionBuilder<TState>,IStateFromBuilder<TState>, IStateToBuilder<TState> 
        where TState : Enum
    {
        
        private readonly List<StateTransitionInfo> _stateTransitions = new List<StateTransitionInfo>();
        private StateTransitionInfo _stateTransitionInfo;
        
        public readonly List<TState> DisabledSameStateTransitioned = new List<TState>();
        
        public IStateToBuilder<TState> From(TState state)
        {
            //TODO: get existing StateTransitionInfo and use if exists
            _stateTransitionInfo = new StateTransitionInfo
            {
                From = (int) Enum.ToObject(state.GetType(), state)
            };
            _stateTransitions.Add(_stateTransitionInfo);
            return this;
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = (int)Enum.ToObject(state.GetType(), state);
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

        public void OnEnter(TState state, OnStateEnterHandler<TState> handler)
        {
            var stateTransitionInfo = new StateTransitionInfo
            {
                From = -1,
                To = (int)Enum.ToObject(state.GetType(), state)
            };
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnEnter), 
                HandlerType.OnEnter));
            _stateTransitions.Add(stateTransitionInfo);
        }

        public void OnEnter<T>(TState state, T target, Expression<Func<T, OnStateEnterHandler<TState>>> handler)
        {
            var stateTransitionInfo = new StateTransitionInfo
            {
                From = -1,
                To = (int)Enum.ToObject(state.GetType(), state)
            };
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEnter, target), 
                HandlerType.OnEnter));
            _stateTransitions.Add(stateTransitionInfo);
        }

        public void OnExit(TState state, OnStateExitHandler<TState> handler)
        {
            var stateTransitionInfo = new StateTransitionInfo
            {
                From = (int)Enum.ToObject(state.GetType(), state),
                To = -1
            };
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler, HandlerType.OnExit), 
                HandlerType.OnExit));
            _stateTransitions.Add(stateTransitionInfo);
        }

        public void OnExit<T>(TState state, T target, Expression<Func<T, OnStateExitHandler<TState>>> handler)
        {
            var stateTransitionInfo = new StateTransitionInfo
            {
                From = (int)Enum.ToObject(state.GetType(), state),
                To = -1
            };
            stateTransitionInfo.Handlers.Add((
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExit, target), 
                HandlerType.OnExit));
            _stateTransitions.Add(stateTransitionInfo);
        }

        public void DisableSameStateTransitionFor(params TState[] states)
        {
            DisabledSameStateTransitioned.AddRange(states);
        }

        internal ActionInfo[] GetOnTransitioningHandlers()
        {
            //TODO: optimize
            return _stateTransitions.SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == HandlerType.OnTransitioning)
                .Select(a => a.Item1).ToArray();
        }

        internal ActionInfo[] GetOnTransitionedHandlers()
        {
            return GetHandlers(HandlerType.OnTransitioned);
        }

        internal ActionInfo[] GetOnExitHandlers()
        {
            return GetHandlers(HandlerType.OnExit);
        }

        internal ActionInfo[] GetOnEnterHandlers()
        {
            return GetHandlers(HandlerType.OnEnter);
        }

        public ActionInfo<TState>[] GetOnEnterGuardHandlers()
        {
            return GetHandlers(HandlerType.OnEnterGuard)
                .Select(a => (ActionInfo<TState>)a).ToArray();
        }

        private ActionInfo[] GetHandlers(HandlerType handlerType)
        {
            //TODO: optimize
            return _stateTransitions.SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == handlerType)
                .Select(a => a.Item1).ToArray();
        }

        public TState[] GetNextStates(TState state)
        {
            //TODO: optimize
            var stateFilter = (int)Enum.ToObject(state.GetType(), state);
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => (TState)(object)a.To).ToArray();
        }

        internal void AddOnStateEnterGuards<TContext>(TState state, TContext context, IEnumerable<OnStateEnterGuardHandler<TState,TContext>> guards) 
            where TContext : GuardContext<TState>
        {
            var stateTransitionInfo = new StateTransitionInfo
            {
                From = -1,
                To = (int)Enum.ToObject(state.GetType(), state)
            };
            foreach (var handler in guards)
            {
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TContext>(context, handler, HandlerType.OnEnterGuard), 
                    HandlerType.OnEnterGuard));
            }
            _stateTransitions.Add(stateTransitionInfo);
        }
    }
}