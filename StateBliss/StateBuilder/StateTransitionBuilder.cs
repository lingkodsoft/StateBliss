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

        public void TriggerTo(TState nextState, string trigger)
        {
            Triggers.Add((trigger, null, nextState.ToInt()));
        }

        public void OnEntered<TTriggerCommand>(TState state, OnStateEventHandler<TState, TTriggerCommand> handler)
            where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                new ActionInfo<TState, TTriggerCommand>(null, handler, HandlerType.OnEntered));
        }

        public void OnEntered(TState state, OnStateEventHandler<TState, TriggerCommand<TState>> handler)
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                new ActionInfo<TState, TriggerCommand<TState>>(null, handler, HandlerType.OnEntered));
        }

        public void OnEntered<TTriggerCommand, TContext>(TState state, Func<TContext> contextProvider,
            params OnStateEventHandler<TState, TTriggerCommand, TContext>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new()
        {
            foreach (var handler in handlers)
                AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                    new ActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider, handler, HandlerType.OnEntered));
        }

        public void OnEntered<TTriggerCommand>(TState state, params OnStateEventHandler<TState, TTriggerCommand, StateContext<TState>>[] handlers) where TTriggerCommand : TriggerCommand<TState>, new()
        {
            var context = new StateContext<TState>();
            foreach (var handler in handlers)
                AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                    new ActionInfo<TState, TTriggerCommand, StateContext<TState>>(null, () => context, handler, HandlerType.OnEntered));
        }

        public void OnEntered<TTriggerCommand, T>(TState state, T target, Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand>>> handler)
            where T : class
            where TTriggerCommand : TriggerCommand<TState>
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                new ActionInfo<TState, TTriggerCommand>(null, handler.GetMethodName(), HandlerType.OnEntered, target));
        }

        public void OnEntered<T>(TState state, T target, Expression<Func<T, OnStateEventHandler<TState, TriggerCommand<TState>>>> handler)
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                new ActionInfo<TState, TriggerCommand<TState>>(null, handler.GetMethodName(), HandlerType.OnEntered, target));
        }

        public void OnEntered<TTriggerCommand, T, TContext>(TState state, T target, Func<TContext> contextProvider,
            params Expression<Func<T, OnStateEventHandler<TState, TTriggerCommand, TContext>>>[] handlers)
            where TContext : new()
            where TTriggerCommand : TriggerCommand<TState>, new()
        {
            foreach (var handler in handlers)
                AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                    new ActionInfo<TState, TTriggerCommand, TContext>(null, contextProvider, handler.GetMethodName(), HandlerType.OnEntered, target));
        }

        public void DisableSameStateTransitionFor(params TState[] states)
        {
            _stateHandlerDefinition.AddDisabledSameStateTransitions(states);
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = state.ToInt();
            _stateHandlerDefinition.AddTransition(_stateTransitionInfo);
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

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand, T>(T target,
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

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand, T, TContext>(T target,
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

        public IStateTransitionBuilder<TState> Changing<TTriggerCommand>(TState state, 
            params OnStateEventHandler<TState, TTriggerCommand, GuardStateContext<TState>>[] handlers)
            where TTriggerCommand : TriggerCommand<TState>, new()
        {
            var context = new GuardStateContext<TState>();
            foreach (var handler in handlers)
                AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                    new ActionInfo<TState, TTriggerCommand, GuardStateContext<TState>>(null, () => context, handler, HandlerType.OnEntered));
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

        public IStateTransitionBuilder<TState> Changed<TTriggerCommand, T>(T target,
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

        public IStateTransitionBuilder<TState> Changed<TTriggerCommand, T, TContext>(T target,
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
            Triggers.Add((trigger, _stateTransitionInfo.From, _stateTransitionInfo.To));
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

            foreach (var handler in handlers)
                stateTransitionInfo.Handlers.Add((
                    new ActionInfo<TState, TTriggerCommand>(context, handler, handlerType),
                    handlerType));
        }

        public void SetContext(object context)
        {
            foreach (var stateTransitionInfo in _stateHandlerDefinition.Transitions)
            foreach (var handlerInfo in stateTransitionInfo.Handlers)
                handlerInfo.Item1.Context = context;
        }

        public void SetCommand(TriggerCommand command)
        {
            foreach (var stateTransitionInfo in _stateHandlerDefinition.Transitions)
            foreach (var handlerInfo in stateTransitionInfo.Handlers)
                handlerInfo.Item1.Command = command;
        }
    }
}