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
        private ConcurrentDictionary<Guid, WeakReference<State>> _managedStates = new ConcurrentDictionary<Guid, WeakReference<State>>();
        private bool _stopRunning;
        private Task _taskRunner;
        private CancellationTokenSource _taskRunnercts;
        private SpinWait _spinWait = new SpinWait();
        private StateFactory _stateFactory;

        private static readonly List<WeakReference<StateMachineManager>> _managers = new List<WeakReference<StateMachineManager>>();
        private static readonly StateMachineManager _default = new StateMachineManager();
        public static StateMachineManager Default => _default;

        public StateMachineManager()
        {
            _managers.Add(new WeakReference<StateMachineManager>(this));
            RemoveDereferencedManagers();
        }

        public event EventHandler<(Exception exception, State state, int fromState, int toState)> OnHandlerException; 

        public void Register(State state)
        {
            Start();
            UnregisterStateFromDefaultInstance(state);
            state.Manager = this;
            
            if (!_managedStates.ContainsKey(state.Id))
            {
                var value = new WeakReference<State>(state);
                while (!_managedStates.TryAdd(state.Id, value))
                {
                    _spinWait.SpinOnce();
                } 
            }
            RemovedDereferenceStatesFromDefaultInstance();
        }

        public static void Trigger(string triggerName)
        {
            foreach (var wrManager in _managers)
            {
                if (wrManager.TryGetTarget(out var manager))
                {
                    manager.TriggerStateChange(triggerName);
                }
            }
        }
        
        private void TriggerStateChange(string triggerName)
        {
            foreach (var wrState in _managedStates.Values)
            {
                if (wrState.TryGetTarget(out var s))
                {
                    var triggerInfo = s.StateTransitionBuilder.Triggers.SingleOrDefault(a => a.trigger == triggerName);

                    if (triggerInfo.fromState == null || triggerInfo.state.Current == triggerInfo.fromState)
                    {
                        triggerInfo.state?.ChangeTo(triggerInfo.toState);
                    }
                }
            }
        }
        
        public static void Guards<TState, TContext>(Guid id, TState state, TContext context, GuardsInfo<TState, GuardContext<TState>> @from) where TState : Enum
            where TContext : GuardContext<TState>
        {
            var state1 = GetState<TState>(id);
            state1.StateTransitionBuilder.AddOnStateEnterGuards(state, context,  @from.Guards);
        }

        private void RemoveDereferencedManagers()
        {
            _managers.RemoveAll(a => !a.TryGetTarget(out var s));
        }
        
        private void UnregisterStateFromDefaultInstance(State state)
        {
            var defaultStateManager = (StateMachineManager)Default;
            defaultStateManager._managedStates.TryRemove(state.Id, out var s);
        }
        
        private void RemovedDereferenceStatesFromDefaultInstance()
        {
            var defaultStateManager = (StateMachineManager)Default;
            foreach (var kv in defaultStateManager._managedStates)
            {
                if (!kv.Value.TryGetTarget(out var state))
                {
                    defaultStateManager._managedStates.TryRemove(kv.Key, out var s);
                }
            }
        }

        private void QueueActionForExecution(ActionInfo actionInfo, State state, int fromState, int toState)
        {
            _actionInfos.Add((actionInfo, state, fromState, toState));
        }

        public void Start()
        {
            if (_taskRunner != null)
            {
                return;
            }
            _stopRunning = false;
            _taskRunnercts = new CancellationTokenSource();
            _taskRunner = Task.Factory.StartNew(Process, _taskRunnercts.Token);
        }

        public void Stop()
        {
            _taskRunnercts?.Cancel();
            _stopRunning = true;
            _actionInfos.Add((null, null, -1, -1));
        }

        private void Process()
        {
            foreach (var item in _actionInfos.GetConsumingEnumerable())
            {
                if (_stopRunning)
                {
                    break;
                }

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

        public async Task WaitAllHandlersProcessed(int waitDelayMilliseconds = 1000)
        {
            var spin = new SpinWait();
            while (_actionInfos.Count > 0)
            {
                spin.SpinOnce();
            }

            await Task.Delay(waitDelayMilliseconds);
        }
        
        public bool ChangeState<TEntity, TState>(State<TEntity, TState> state, TState newState) where TState : Enum
        {
            return ChangeState<TState>(state, newState);
        }
        
        public static void SetDefaultStateFactory(StateFactory stateFactory)
        {
            Default.SetStateFactory(stateFactory);
        }

        public void SetStateFactory(StateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }
        
        public static bool ChangeState<TState>(TState newState, Guid id) where TState : Enum
        {
            return Default.ChangeStateInternal(newState, id);
        }
        
        public static State<TState> GetState<TState>(Guid id) where TState : Enum
        {
            return (State<TState>) Default._stateFactory(typeof(TState), id);
        }
        
        State<TState> IStateMachineManager.GetState<TState>(Guid id)
        {
            return (State<TState>)_stateFactory(typeof(TState), id);
        }
        
        private bool ChangeStateInternal<TState>(TState newState, Guid id) where TState : Enum
        {
            return ChangeState((State<TState>) _stateFactory(typeof(TState), id), newState);
        }
        
        bool IStateMachineManager.ChangeState<TState>(TState newState, Guid id)
        {
            return ChangeStateInternal(newState, id);
        }
        
        public bool ChangeState<TState>(State<TState> state, TState newState) where TState : Enum
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
                try
                {
                    actionInfo.Execute(state, @from, @to);
                }
                catch (Exception e)
                {
                    OnHandlerException?.Invoke(this, (e, state, @from, @to));
                    throw;
                }
            }
            
            //OnExit of current state
            foreach (var actionInfo in stateTransitionBuilder.GetOnExitHandlers())
            {
                QueueActionForExecution(actionInfo, state, @from, @to);
            }

            //OnEnterGuards of new state
            foreach (var actionInfo in stateTransitionBuilder.GetOnEnterGuardHandlers())
            {
                try
                {
                    actionInfo.GuardContext.Continue = false;
                    actionInfo.Execute(state, @from, @to);

                    if (actionInfo.GuardContext.Continue) continue;
                    
                    if (actionInfo.GuardContext.ThrowExceptionWhenDiscontinued)
                    {
                        throw new StateEnterGuardHandlerDiscontinuedException();
                    }
                    return false;
                }
                catch (Exception e)
                {
                    OnHandlerException?.Invoke(this, (e, state, @from, @to));
                    throw;
                }
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

            return true;
        }
    }
}