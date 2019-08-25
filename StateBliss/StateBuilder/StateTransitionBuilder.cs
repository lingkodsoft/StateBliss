using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StateBliss
{

    internal class StateTransitionBuilder<TState> : IStateTransitionBuilder<TState>,
        IStateFromBuilder<TState>, IStateToBuilder<TState>
        where TState : Enum
    {
        private readonly StateDefinition<TState> _stateDefinition;
        private StateTransitionInfo _stateTransitionInfo;

        internal StateTransitionBuilder(StateDefinition<TState> stateDefinition)
        {
            _stateDefinition = stateDefinition;
        }

        public IStateToBuilder<TState> From(TState state)
        {
            _stateTransitionInfo = new StateTransitionInfo
            {
                From = state.ToInt()
            };
            return this;
        }

        public void OnEntering<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntering,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEntering, target));
        }

        public void OnEntered<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(-1, state.ToInt(), HandlerType.OnEntered,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEntered, target));
        }

        public void OnExiting<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            AddHandler(state.ToInt(), -1, HandlerType.OnExiting,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExiting, target));
        }

        public void OnExited<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(state.ToInt(), -1, HandlerType.OnExited,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExited, target));
        }

        public void OnEditing<T>(TState state, T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            var editState = state.ToInt();
            AddHandler(editState, editState, HandlerType.OnEditing,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEditing, target));
        }

        public void OnEdited<T>(TState state, T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            var editState = state.ToInt();
            AddHandler(editState, editState, HandlerType.OnEdited,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEdited, target));
        }

        public void DisableSameStateTransitionFor(params TState[] states)
        {
            _stateDefinition.AddDisabledSameStateTransitions(states.Select(a => a.ToInt()).ToArray());
        }

        public bool ThrowExceptionWhenDiscontinued
        {
            get => _stateDefinition.ThrowExceptionWhenDiscontinued;
            set => _stateDefinition.ThrowExceptionWhenDiscontinued = value;
        }

        public IStateTransitionBuilder<TState> To(TState state)
        {
            _stateTransitionInfo.To = state.ToInt();
            _stateDefinition.AddTransition(_stateTransitionInfo);
            return this;
        }
        
        public IStateTransitionBuilder<TState> Entering<T>(T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            AddHandler(-1, _stateTransitionInfo.To, HandlerType.OnEntering,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEntering, target));
            return this;
        }
        
        public IStateTransitionBuilder<TState> Entered<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(-1, _stateTransitionInfo.To, HandlerType.OnEntered,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnEntered, target));
            return this;
        }

        public IStateTransitionBuilder<TState> Changing<T>(T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanging,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnChanging, target));
            return this;
        }
        
        public IStateTransitionBuilder<TState> Changed<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, _stateTransitionInfo.To, HandlerType.OnChanged,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnChanged, target));
            return this;
        }
     
        public IStateTransitionBuilder<TState> Exiting<T>(T target, Expression<Func<T, StateChangeGuardHandler<TState>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, -1, HandlerType.OnExiting,
                new GuardActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExiting, target));
            return this;
        }
        
        public IStateTransitionBuilder<TState> Exited<T>(T target, Expression<Func<T, StateChangeHandler<TState>>> handler) where T : class
        {
            AddHandler(_stateTransitionInfo.From, -1, HandlerType.OnExited,
                new ActionInfo<TState>(handler.GetMethodName(), HandlerType.OnExited, target));
            return this;
        }
        
        private void AddHandler(int fromState, int toState, HandlerType handlerType, ActionInfo actionInfo)
        {
            var stateTransitionInfo = _stateDefinition.Transitions.SingleOrDefault(a => a.From == fromState && a.To == toState);
            if (stateTransitionInfo == null)
            {
                stateTransitionInfo = new StateTransitionInfo
                {
                    From = fromState,
                    To = toState
                };
                _stateDefinition.AddTransition(stateTransitionInfo);
            }

            stateTransitionInfo.Handlers.Add((actionInfo, handlerType));
        }
    }
}