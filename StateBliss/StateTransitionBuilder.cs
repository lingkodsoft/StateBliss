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

        public IStateTransitionBuilder<TState> OnTransitioned<T>(T target, Expression<Func<T, OnStateTransitionedHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioned, target), 
                HandlerType.OnTransitioned));
            return this;
        }

        public IStateTransitionBuilder<TState> OnTransitioning<T>(T target, Expression<Func<T, OnStateTransitioningHandler<TState>>> handler)
        {
            _stateTransitionInfo.Handlers.Add( (
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnTransitioning, target), 
                HandlerType.OnTransitioning));
            return this;
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

        internal IReadOnlyList<ActionInfo> GetOnTransitioningHandlers()
        {
            //TODO: optimize
            return _stateTransitions.SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == HandlerType.OnTransitioning)
                .Select(a => a.Item1).ToList();
        }

        internal IReadOnlyList<ActionInfo> GetOnTransitionedHandlers()
        {
            return GetHandlers(HandlerType.OnTransitioned);
        }

        internal IReadOnlyList<ActionInfo> GetOnExitHandlers()
        {
            return GetHandlers(HandlerType.OnExit);
        }

        internal IReadOnlyList<ActionInfo> GetOnEnterHandlers()
        {
            return GetHandlers(HandlerType.OnEnter);
        }

        private IReadOnlyList<ActionInfo> GetHandlers(HandlerType handlerType)
        {
            //TODO: optimize
            return _stateTransitions.SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == handlerType)
                .Select(a => a.Item1).ToList();
        }

        public TState[] GetNextStates(TState state)
        {
            //TODO: optimize
            var stateFilter = (int)Enum.ToObject(state.GetType(), state);
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => (TState)(object)a.To).ToArray();
        }
    }
}