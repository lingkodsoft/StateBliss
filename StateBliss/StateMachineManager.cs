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
        private ConcurrentQueue<(ActionInfo actionInfo, State state, int fromState, int toState)> _actionInfos = new ConcurrentQueue<(ActionInfo, State state, int fromState, int toState)>();
        private ConcurrentDictionary<Guid, WeakReference<State>> _managedStates = new ConcurrentDictionary<Guid, WeakReference<State>>();
        private volatile bool _stopRunning;
        private Task _taskRunner;
        private CancellationTokenSource _taskRunnercts;
        private SpinWait _spinWait = new SpinWait();
        private StateFactory _stateFactory;
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private static readonly List<WeakReference<StateMachineManager>> _managers = new List<WeakReference<StateMachineManager>>();
        private static readonly StateMachineManager _default = new StateMachineManager();
        public static StateMachineManager Default => _default;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

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
            _actionInfos.Enqueue((actionInfo, state, fromState, toState));
            _resetEvent.Set();
        }

        private object _taskLock = new object();
        public void Start()
        {
            lock (_taskLock)
            {
                if (_taskRunner?.Status == TaskStatus.Running)
                {
                    return;
                }

                _stopRunning = false;
                _taskRunnercts = new CancellationTokenSource();
                _taskRunner = Task.Factory.StartNew(Process, _taskRunnercts.Token);
            }
        }

        public void Stop()
        {
            _taskRunnercts?.Cancel();
            _stopRunning = true;
            _resetEvent.Set();
        }

        private void Process()
        {
            while(true)
            {
                lock (_taskLock)
                {
                    if (_stopRunning)
                    {
                        _taskRunner = null;
                        _resetEvent.Set();
                        return;
                    }
                }
                
                if (!_actionInfos.TryDequeue(out var item))
                {
                    _resetEvent.WaitOne();
                    _resetEvent.Reset();
                    continue;
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

        public async Task WaitAllHandlersProcessedAsync(int waitDelayMilliseconds = 100)
        {
            var spin = new SpinWait();
            while (!_actionInfos.IsEmpty)
            {
                spin.SpinOnce();
            }
            if (waitDelayMilliseconds == 0) return;
            await Task.Delay(waitDelayMilliseconds).ConfigureAwait(false);
        }

        public void WaitAllHandlersProcessed(int waitDelayMilliseconds = 100)
        {
            var spin = new SpinWait();
            while (!_actionInfos.IsEmpty)
            {
                spin.SpinOnce();
            }
            var startTime = DateTime.Now;
            while(DateTime.Now.Subtract(startTime).TotalMilliseconds < waitDelayMilliseconds)
            {
                spin.SpinOnce();
            }
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
            return GetStateInternal<TState>(id);
        }
        
        private State<TState> GetStateInternal<TState>(Guid id) where TState : Enum
        {
            return (State<TState>)_stateFactory(typeof(TState), id);
        }
        
        private bool ChangeStateInternal<TState>(TState newState, Guid id) where TState : Enum
        {
            return ChangeState(GetStateInternal<TState>(id), newState);
        }
        
        bool IStateMachineManager.ChangeState<TState>(TState newState, Guid id)
        {
            return ChangeStateInternal(newState, id);
        }
        
        public bool ChangeState<TState>(State<TState> state, TState newState) where TState : Enum
        {
            var stateTransitionBuilder = state.StateTransitionBuilder;

            var curentState = state.Current;
            int @from;
            int @to;

            _lock.EnterReadLock();
            try
            {
                @from = state.Current.ToInt();
                @to = newState.ToInt();

                //trigger handlers
                if (@from == @to)
                {
                    if (stateTransitionBuilder.DisabledSameStateTransitioned.Any(a => a.Equals(newState)))
                    {
                        throw new SameStateTransitionDisabledException();
                    }

                    //On edit guards
                    foreach (var actionInfo in stateTransitionBuilder.GetOnEditGuardHandlers(curentState))
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
                    
                    //On Edited state
                    foreach (var actionInfo in stateTransitionBuilder.GetOnEditHandlers(curentState))
                    {
                        QueueActionForExecution(actionInfo, state, @from, @to);
                    }

                }
                else
                {
                    var nextStateTransitions = state.GetNextStates();
                    if (!nextStateTransitions.Any(a => a.Equals(newState)))
                    {
                        throw new NoRuleDefinedForStateTransitionException();
                    }
                }

                //OnExitGuards of new state
                foreach (var actionInfo in stateTransitionBuilder.GetOnExitGuardHandlers(curentState))
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

                //OnEnterGuards of new state
                foreach (var actionInfo in stateTransitionBuilder.GetOnEnterGuardHandlers(newState))
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

                //OnTransitioning
                foreach (var actionInfo in stateTransitionBuilder.GetOnTransitioningHandlers(curentState, newState))
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
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                state.SetEntityState(newState.ToInt());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            _lock.EnterReadLock();
            try
            {
                //OnExit of current state
                foreach (var actionInfo in stateTransitionBuilder.GetOnExitHandlers(curentState))
                {
                    QueueActionForExecution(actionInfo, state, @from, @to);
                }

                //OnEnter of new state
                foreach (var actionInfo in stateTransitionBuilder.GetOnEnterHandlers(newState))
                {
                    QueueActionForExecution(actionInfo, state, @from, @to);
                }

                //OnTransitioned
                foreach (var actionInfo in stateTransitionBuilder.GetOnTransitionedHandlers(curentState, newState))
                {
                    QueueActionForExecution(actionInfo, state, @from, @to);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return true;
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _taskRunner?.Dispose();
                _taskRunnercts?.Dispose();
                _resetEvent?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StateMachineManager()
        {
            Dispose(false);
        }
    }
}