using System;
using System.Collections.Generic;
using System.Linq;

namespace StateBliss
{
    public abstract class StateHandlerDefinition<TState> : IStateDefinition where TState : Enum
    {
        private readonly List<StateTransitionInfo> _stateTransitions = new List<StateTransitionInfo>();
        private readonly List<TState> _disabledSameStateTransition = new List<TState>();
        
        private StateTransitionBuilder<TState> StateTransitionBuilder;
        

        internal IReadOnlyList<TState> DisabledSameStateTransition => _disabledSameStateTransition;
        internal StateTransitionBuilder<TState> Builder { get; set; }
        public IReadOnlyList<StateTransitionInfo> Transitions => _stateTransitions;

        void IStateDefinition.Define()
        {
            if (StateTransitionBuilder != null)
            {
                throw new InvalidOperationException($"Define must be called once only.");
            }
            StateTransitionBuilder = new StateTransitionBuilder<TState>(this);
            Define(StateTransitionBuilder);
        }

        public abstract void Define(IStateFromBuilder<TState> builder);
        
        
        internal void AddDisabledSameStateTransitions(TState[] states)
        {
            _disabledSameStateTransition.AddRange(states);
        }

        internal void AddTransition(StateTransitionInfo stateTransitionInfo)
        {
            _stateTransitions.Add(stateTransitionInfo);
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

        internal ActionInfo[] GetOnEnteredHandlers(int toState)
        {
            return GetHandlers(HandlerType.OnEntered, null, toState);
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

        internal TState[] GetNextStates(TState state)
        {
            var stateFilter = state.ToInt();
            return _stateTransitions.Where(a => a.From == stateFilter)
                .Select(a => a.To.ToEnum<TState>()).ToArray();
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

            return  _stateTransitions
                .Where(filter)
                .SelectMany(a => a.Handlers)
                .Where(a => a.Item2 == handlerType)
                .Select(a => a.Item1).ToArray();
        }
    }
}