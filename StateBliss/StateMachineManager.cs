using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StateBliss
{
    public class StateMachineManager : IStateMachineManager
    {
        private ConcurrentQueue<(ActionInfo actionInfo, StateChangeInfo changeInfo)> _actionInfos = new ConcurrentQueue<(ActionInfo, StateChangeInfo)>();
        private volatile bool _stopRunning;
        private Task _taskRunner;
        private CancellationTokenSource _taskRunnercts;
        private SpinWait _spinWait = new SpinWait();
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private static readonly List<WeakReference<StateMachineManager>> _managers = new List<WeakReference<StateMachineManager>>();
        private static readonly StateMachineManager _default = new StateMachineManager();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public static IStateMachineManager Default => _default;

        public event EventHandler<(Exception exception, StateChangeInfo changeInfo)> OnHandlerException;

        private List<IStateDefinition> _stateDefinitions = new List<IStateDefinition>();
        void IStateMachineManager.Register(IEnumerable<Assembly> assemblyDefinitions, Func<Type, object> serviceProvider = null)
        {
            Start();
            
            foreach (var assembly in assemblyDefinitions)
            {
                foreach (var type in assembly.GetTypes()
                    .Where(a => a.IsClass && typeof(IStateDefinition).IsAssignableFrom(a)))
                {
                    var definition = serviceProvider == null
                        ? (IStateDefinition) Activator.CreateInstance(type)
                        : (IStateDefinition) serviceProvider(type);
                    
                    var definitionHandlerType = typeof(StateDefinition<>).MakeGenericType(definition.EnumType);
                    if (!definitionHandlerType.IsInstanceOfType(definition))
                    {
                        throw new ArgumentException("StateDefinition must be of type StateHandlerDefinition<TState>");
                    }

                    definition.Define();
                    
                    if (!definition.Transitions.Any())
                    {
                        throw new InvalidOperationException("At least one transition must be defined. You may inherit from StateHandlerDefinition<TState> and define transitions");
                    }

                    _stateDefinitions.Add(definition);
                }
            }
        }

        public static void Register(IEnumerable<Assembly> assemblyDefinitions, Func<Type, object> serviceProvider = null)
        {
            Default.Register(assemblyDefinitions, serviceProvider);
        }

        public static StateChangeResult<TState, TData> Trigger<TState, TData>(TState currentState, TState nextState, TData data)
            where TState : Enum
        {
            return Default.Trigger(currentState, nextState, data);
        }

        StateChangeResult<TState, TData> IStateMachineManager.Trigger<TState, TData>(TState currentState, TState nextState, TData data)
        {
            var definition = GetStateDefinition<TState>();
            return ChangeStateInternal(currentState.ToInt(), nextState.ToInt(), definition, data);
        }
        
        TState[] IStateMachineManager.GetNextStates<TState>(TState currentState)
        {
            var definition = GetStateDefinition<TState>();
            return definition.GetNextStates(currentState);
        }
        
        public static TState[] GetNextStates<TState>(TState currentState) where TState : Enum
        {
            return Default.GetNextStates(currentState);
        }
        
        public StateDefinition<TState> GetStateDefinition<TState>() where TState : Enum
        {
            var definitions = _stateDefinitions.Where(a => a.EnumType == typeof(TState)).ToArray();
            var definition = definitions.Count() > 1
                ? new AggregateStateDefinition<TState>(definitions)
                : definitions.Single();
            return (StateDefinition<TState>)definition;
        }

        private void QueueActionForExecution(ActionInfo actionInfo, StateChangeInfo changeInfo)
        {
            _actionInfos.Enqueue((actionInfo, changeInfo));
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
                    item.actionInfo.Execute(item.changeInfo);
                }
                catch (Exception e)
                {
                    OnHandlerException?.Invoke(this, (e, item.changeInfo));
                }
            }
        }

        public async Task WaitAllHandlersProcessedAsync(int waitDelayMilliseconds = 100)
        {
            var startTime = DateTime.Now;
            var spin = new SpinWait();
            while (true)
            {
                if (DateTime.Now.Subtract(startTime).TotalMilliseconds > 1000)
                {
                    if (_actionInfos.IsEmpty)
                    {
                        break;
                    }
                    startTime = DateTime.Now;
                }
                spin.SpinOnce();
            }
            startTime = DateTime.Now;
            if (waitDelayMilliseconds == 0) return;
            await Task.Delay(waitDelayMilliseconds).ConfigureAwait(false);
        }

        public void WaitAllHandlersProcessed(int waitDelayMilliseconds = 100)
        {
            var startTime = DateTime.Now;
            var spin = new SpinWait();
            while (true)
            {
                if (DateTime.Now.Subtract(startTime).TotalMilliseconds > 1000)
                {
                    if (_actionInfos.IsEmpty)
                    {
                        break;
                    }
                    startTime = DateTime.Now;
                }
                spin.SpinOnce();
            }
            startTime = DateTime.Now;
            while(DateTime.Now.Subtract(startTime).TotalMilliseconds < waitDelayMilliseconds)
            {
                spin.SpinOnce();
            }
        }

        private StateChangeResult<TState, TData> ChangeStateInternal<TState, TData>(int fromState, int toState, StateDefinition<TState> stateDefinition, TData data) where TState : Enum
        {
            var changeInfo = new StateChangeGuardInfo<TState>
            {
                FromState = fromState.ToEnum<TState>(),
                ToState = toState.ToEnum<TState>(),
                CurrentState = fromState.ToEnum<TState>(),
                Data = data,
                ThrowExceptionWhenDiscontinued = stateDefinition.ThrowExceptionWhenDiscontinued
            };

            _lock.EnterUpgradeableReadLock();
            try
            {
                //trigger handlers
                if (fromState == toState)
                {
                    if (stateDefinition.DisabledSameStateTransitions.Any(a => a.Equals(toState)))
                    {
                        throw new SameStateTransitionDisabledException();
                    }

                    //On edit guards
                    if (!ExecuteGuardHandlers(changeInfo, stateDefinition.GetOnEditGuardHandlers(fromState)))
                    {
                        return changeInfo.ToResult<TData>();
                    }
                    
                    //On Edited state
                    foreach (var actionInfo in stateDefinition.GetOnEditHandlers(fromState))
                    {
                        QueueActionForExecution(actionInfo, changeInfo);
                    }
                }
                else
                {
                    var nextStateTransitions = stateDefinition.GetNextStates(changeInfo.FromState);
                    if (!nextStateTransitions.Any(a => a.ToInt() == toState))
                    {
                        throw new NoRuleDefinedForStateTransitionException();
                    }
                }

                //OnExitGuards of new state
                if (!ExecuteGuardHandlers(changeInfo, stateDefinition.GetOnExitGuardHandlers(fromState)))
                {
                    return changeInfo.ToResult<TData>();
                }

                //OnEnterGuards of new state
                if (!ExecuteGuardHandlers(changeInfo, stateDefinition.GetOnEnterGuardHandlers(toState)))
                {
                    return changeInfo.ToResult<TData>();
                }

                //OnChanging
                if (!ExecuteGuardHandlers(changeInfo, stateDefinition.GetOnChangingGuardHandlers(fromState, toState)))
                {
                    return changeInfo.ToResult<TData>();
                }

                changeInfo.CurrentState = changeInfo.ToState;
                changeInfo.StateChangedSucceeded = true;

                //OnExit of current state
                foreach (var actionInfo in stateDefinition.GetOnExitHandlers(fromState))
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }

                //OnEnter of new state
                foreach (var actionInfo in stateDefinition.GetOnEnteredHandlers(toState))
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }

                //OnChanged
                var handlers = stateDefinition.GetOnChangedHandlers(fromState, toState).ToArray();
                foreach (var actionInfo in handlers)
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }
                
                return changeInfo.ToResult<TData>();
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            //Release unmanaged resources if any
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

        private bool ExecuteGuardHandlers<TState>(StateChangeGuardInfo<TState> guardChangeInfo, ActionInfo[] handlers) where TState : Enum
        {
            foreach (var actionInfo in handlers)
            {
                try
                {
                    guardChangeInfo.Continue = true;
                    actionInfo.Execute(guardChangeInfo);

                    if (guardChangeInfo.Continue) continue;

                    if (guardChangeInfo.ThrowExceptionWhenDiscontinued)
                    {
                        throw new StateGuardHandlerDiscontinuedException();
                    }

                    return false;
                }
                catch (Exception e)
                {
                    OnHandlerException?.Invoke(this, (e, guardChangeInfo));
                    throw;
                }
            }

            return true;
        }
    }
}