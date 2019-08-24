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
//        private ConcurrentDictionary<Guid, WeakReference<State>> _managedStates = new ConcurrentDictionary<Guid, WeakReference<State>>();
        private volatile bool _stopRunning;
        private Task _taskRunner;
        private CancellationTokenSource _taskRunnercts;
        private SpinWait _spinWait = new SpinWait();
//        private StateFactory _stateFactory;
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private static readonly List<WeakReference<StateMachineManager>> _managers = new List<WeakReference<StateMachineManager>>();
        private static readonly StateMachineManager _default = new StateMachineManager();
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public static IStateMachineManager Default => _default;
        
        public StateMachineManager()
        {
          //  _managers.Add(new WeakReference<StateMachineManager>(this));
          //  RemoveDereferencedManagers();
        }

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
                    
                    var definitionHandlerType = typeof(StateHandlerDefinition<>).MakeGenericType(definition.EnumType);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="nextState"></param>
        /// <param name="data"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns>return true when the state changed successfully</returns>
        public static bool Trigger<TState>(TState currentState, TState nextState, object data)
            where TState : Enum
        {
            return Default.Trigger(currentState, nextState, data);
        }

//        public void Register(IEnumerable<IStateDefinition> definitions)
//        {
//            Start();
////            UnregisterStateFromDefaultInstance(state);
////            state.Manager = this;
//            
////            if (!_managedStates.ContainsKey(state.Id))
////            {
////                var value = new WeakReference<State>(state);
////                while (!_managedStates.TryAdd(state.Id, value))
////                {
////                    _spinWait.SpinOnce();
////                } 
////            }
//            RemovedDereferenceStatesFromDefaultInstance();
//        }

        bool IStateMachineManager.Trigger<TState>(TState currentState, TState nextState, object data)
        {
            var definition = GetStateDefinition<TState>();
            return ChangeStateInternal(currentState.ToInt(), nextState.ToInt(), definition, data);
        }
        
        public StateHandlerDefinition<TState> GetStateDefinition<TState>() where TState : Enum
        {
            var definitions = _stateDefinitions.Where(a => a.EnumType == typeof(TState)).ToArray();
            var definition = definitions.Count() > 1
                ? new AggregateStateHandlerDefinition<TState>(definitions)
                : definitions.Single();
            return (StateHandlerDefinition<TState>)definition;
            //new State<TState>(currentState, (StateHandlerDefinition<TState>) definition, this);
        }
        
        private void RemoveDereferencedManagers()
        {
            _managers.RemoveAll(a => !a.TryGetTarget(out var s));
        }
//        
//        private void UnregisterStateFromDefaultInstance(State state)
//        {
//            _managedStates.TryRemove(state.Id, out var s);
//        }
////        
//        private void RemovedDereferenceStatesFromDefaultInstance()
//        {
//            foreach (var kv in _managedStates)
//            {
//                if (!kv.Value.TryGetTarget(out var state))
//                {
//                    _managedStates.TryRemove(kv.Key, out var s);
//                }
//            }
//        }

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

        public void RegisterHandlerDefinition(IEnumerable<IStateDefinition> handlerDefinitions)
        {
            //TODO: RegisterHandlerDefinition
        }
        
        private bool ChangeStateInternal<TState>(int fromState, int toState, StateHandlerDefinition<TState> stateHandlerDefinition, object data) where TState : Enum
        {
            var changeInfo = new StateChangeInfo<TState>
            {
                FromState = fromState.ToEnum<TState>(),
                ToState = toState.ToEnum<TState>(),
                Data = data,
            };
            
            _lock.EnterUpgradeableReadLock();
            try
            {
                //trigger handlers
                if (fromState == toState)
                {
                    if (stateHandlerDefinition.DisabledSameStateTransitions.Any(a => a.Equals(toState)))
                    {
                        throw new SameStateTransitionDisabledException();
                    }

                    //On edit guards
                    if (!ExecuteGuardHandlers(changeInfo, stateHandlerDefinition.GetOnEditGuardHandlers(fromState)))
                    {
                        return false;
                    }
                    
                    //On Edited state
                    foreach (var actionInfo in stateHandlerDefinition.GetOnEditHandlers(fromState))
                    {
                        QueueActionForExecution(actionInfo, changeInfo);
                    }
                }
                else
                {
                    var nextStateTransitions = stateHandlerDefinition.GetNextStates(changeInfo.FromState);
                    if (!nextStateTransitions.Any(a => a.ToInt() == toState))
                    {
                        throw new NoRuleDefinedForStateTransitionException();
                    }
                }

                //OnExitGuards of new state
                if (!ExecuteGuardHandlers(changeInfo, stateHandlerDefinition.GetOnExitGuardHandlers(fromState)))
                {
                    return false;
                }

                //OnEnterGuards of new state
                if (!ExecuteGuardHandlers(changeInfo, stateHandlerDefinition.GetOnEnterGuardHandlers(toState)))
                {
                    return false;
                }

                //OnChanging
                if (!ExecuteGuardHandlers(changeInfo, stateHandlerDefinition.GetOnChangingGuardHandlers(fromState, toState)))
                {
                    return false;
                }
                
//                state.SetEntityState(newState.ToInt());
                

                //OnExit of current state
                foreach (var actionInfo in stateHandlerDefinition.GetOnExitHandlers(fromState))
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }

                //OnEnter of new state
                foreach (var actionInfo in stateHandlerDefinition.GetOnEnteredHandlers(toState))
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }

                //OnChanged
                var handlers = stateHandlerDefinition.GetOnChangedHandlers(fromState, toState).ToArray();
                foreach (var actionInfo in handlers)
                {
                    QueueActionForExecution(actionInfo, changeInfo);
                }
                
                return true;                
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
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
        
        private bool ExecuteGuardHandlers<TState>(StateChangeInfo<TState> changeInfo, ActionInfo[] handlers) where TState : Enum
        {
            foreach (var actionInfo in handlers)
            {
                try
                {
                    changeInfo.Continue = true;
                    actionInfo.Execute(changeInfo);

                    if (changeInfo.Continue) continue;

                    if (changeInfo.ThrowExceptionWhenDiscontinued)
                    {
                        throw new StateEnterGuardHandlerDiscontinuedException();
                    }

                    return false;
                }
                catch (Exception e)
                {
                    OnHandlerException?.Invoke(this, (e, changeInfo));
                    throw;
                }
            }

            return true;
        }
    }
}