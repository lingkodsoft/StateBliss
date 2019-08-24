using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateBliss
{
    internal class StateTransitionBuilder
    {
        internal readonly List<(string trigger, int? fromState, int toState)> Triggers =
            new List<(string trigger, int? fromState, int toState)>();

        //protected State _state;
    }

    internal class StateTransitionBuilder<TState> : StateTransitionBuilder, IStateTransitionBuilder<TState>,
        IStateFromBuilder<TState>, IStateToBuilder<TState>
        where TState : Enum
    {
        private readonly StateHandlerDefinition<TState> _stateHandlerDefinition;
        private StateTransitionInfo _stateTransitionInfo;

        internal StateTransitionBuilder(StateHandlerDefinition<TState> stateHandlerDefinition)
        {
            _stateHandlerDefinition = stateHandlerDefinition;
        }

        public IStateToBuilder<TState> From(TState state)
        {
            _stateTransitionInfo = new StateTransitionInfo
            {
                From = state.ToInt()
            };
            return this;
        }

        public void DisableSameStateTransitionFor(params TState[] states)
        {
            _stateHandlerDefinition.AddDisabledSameStateTransitions(states.Select(a => a.ToInt()).ToArray());
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = state.ToInt();
            _stateHandlerDefinition.AddTransition(_stateTransitionInfo);
            return this;
        }

     

        public IStateTransitionBuilder<TState> Changing<T, TTriggerContext>(T target, Expression<Func<T, StateChangeHandler<TState, TTriggerContext>>> handler) 
            where T : class
        {
            throw new NotImplementedException();
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnChanging, target));
            return this;
        }

     
        private void AddHandler(int fromState, int toState, HandlerType handlerType, ActionInfo actionInfo)
        {
            var stateTransitionInfo = _stateHandlerDefinition.Transitions.SingleOrDefault(a => a.From == fromState && a.To == toState);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = fromState,
                    To = toState
                };
                _stateHandlerDefinition.AddTransition(stateTransitionInfo);
            }

            stateTransitionInfo.Handlers.Add((actionInfo, handlerType));
        }

        private void AddHandlerWithContext<TTriggerCommand>(HandlerType handlerType, int fromState, int toState,
            TTriggerCommand context,
            IEnumerable<OnStateEventHandler<TState, TTriggerCommand>> handlers)
            where TTriggerCommand : TriggerCommand<TState>
        {
            var stateType = typeof(TState);
            if (context != null && context.StateType != stateType)
                throw new StateTypeMismatchException(
                    $"The type {context.StateType.FullName} is not matched to the required type {stateType.FullName}");

            var stateTransitionInfo = _stateHandlerDefinition.Transitions.SingleOrDefault(a => a.From == fromState && a.To == toState);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = fromState,
                    To = toState
                };
                _stateHandlerDefinition.AddTransition(stateTransitionInfo);
            }

//            foreach (var handler in handlers)
//                stateTransitionInfo.Handlers.Add((
//                    new ActionInfo<TState>(handler, handlerType),
//                    handlerType));
//            
//            
        }

        public void SetContext(object context)
        {
            foreach (var stateTransitionInfo in _stateHandlerDefinition.Transitions)
            foreach (var handlerInfo in stateTransitionInfo.Handlers)
                handlerInfo.Item1.Context = context;
        }
//
//        public void SetCommand(TriggerCommand command)
//        {
//            foreach (var stateTransitionInfo in _stateHandlerDefinition.Transitions)
//            foreach (var handlerInfo in stateTransitionInfo.Handlers)
//                handlerInfo.Item1.Command = command;
//        }
    }
}