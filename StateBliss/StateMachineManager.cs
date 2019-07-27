using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateBliss
{
    public class StateMachineManager : IStateMachineManager
    {
        private BlockingCollection<(ActionInfo actionInfo, State state, int fromState, int toState)> _actionInfos = new BlockingCollection<(ActionInfo, State state, int fromState, int toState)>();
        private List<State> _managedStates = new List<State>();
        private bool _stopRunning;
        private Task _taskRunner;
        private CancellationTokenSource _taskRunnercts;

        public event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException; 

        public void Register(State state)
        {
            state.Manager = this;
            _managedStates.Add(state);
        }

        private void QueueActionForExecution(ActionInfo actionInfo, State state, int fromState, int toState)
        {
            _actionInfos.Add((actionInfo, state, fromState, toState));
        }

        public void Start()
        {
            _stopRunning = false;
            _taskRunnercts = new CancellationTokenSource();
            _taskRunner = Task.Factory.StartNew(Process, _taskRunnercts.Token);
        }

        public void Stop()
        {
            _taskRunnercts.Cancel();
            _stopRunning = true;
        }

        private void Process()
        {
            while (!_stopRunning)
            {
                foreach (var item in _actionInfos.GetConsumingEnumerable())
                {
                    try
                    {
                        item.actionInfo.Execute(item.state, item.fromState, item.toState);
                    }
                    catch (Exception e)
                    {
                        OnHandlerException?.Invoke(this, (e, item.state, item.fromState, item.toState));
                    }
                }
            }
        }

        public async Task WaitAllHandlersProcessed()
        {
            var spin = new SpinWait();
            while (_actionInfos.Count > 0)
            {
                spin.SpinOnce();
            }

            await Task.Delay(2000);
        }

        public void ChangeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum
        {
            var @from = (int)Enum.ToObject(state.Current.GetType(), state.Current);
            var @to = (int)Enum.ToObject(newState.GetType(), newState);

            //trigger handlers
            var stateTransitionBuilder = state.StateTransitionBuilder;

            if (Equals(state.Current, newState))
            {
                if (stateTransitionBuilder.DisabledSameStateTransitioned.Any(a => Equals(a, newState)))
                {
                    throw new SameStateTransitionDisabledException();
                }
            }
            else
            {
                var nextStateTransitions = state.GetNextStates();
                if (!nextStateTransitions.Any(a => Equals(a, newState)))
                {
                    throw new NoRuleDefinedForStateTransitionException();
                }
            }

            //OnTransitioning
            foreach (var actionInfo in stateTransitionBuilder.GetOnTransitioningHandlers())
            {
                actionInfo.Execute(state, @from, @to);
            }
            
            //OnExit of current state
            foreach (var actionInfo in stateTransitionBuilder.GetOnExitHandlers())
            {
                QueueActionForExecution(actionInfo, state, @from, @to);
            }

            //OnEnter of new state
            foreach (var actionInfo in stateTransitionBuilder.GetOnEnterHandlers())
            {
                QueueActionForExecution(actionInfo, state, @from, @to);
            }

            state.SetEntityState((int)Enum.ToObject(newState.GetType(), newState));
            
            //OnTransitioned
            foreach (var actionInfo in stateTransitionBuilder.GetOnTransitionedHandlers())
            {
                QueueActionForExecution(actionInfo, state, @from, @to);
            }
        }
    }
}