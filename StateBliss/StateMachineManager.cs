using System;
using System.Collections.Concurrent;

namespace StateBliss
{
    public class StateMachineManager : IStateMachineManager
    {
        private BlockingCollection<ActionInfo> _actionInfos = new BlockingCollection<ActionInfo>();
        
        public void Register(State state)
        {
            throw new System.NotImplementedException();
        }

        private void QueueActionForExecution(ActionInfo actionInfo)
        {
            
        }


        private void Process()
        {
            while (true)
            {
                foreach (var actionInfo in _actionInfos.GetConsumingEnumerable())
                {
                    try
                    {
                        actionInfo.Execute();
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
        }
        
        public void ChamgeState<TStatus>(State state, TStatus newState) where TStatus : Enum
        {
            //trigger handlers
            
            //OnTransitioning
            var onTransitioningHandlers = state.GetOnTransitioningHandlers(newState);
            foreach (var actionInfo in onTransitioningHandlers)
            {
                actionInfo.Execute();
            }
            
            //OnExit of current state
            var onExitHandlers = state.GetOnExitHandlers();
            foreach (var actionInfo in onExitHandlers)
            {
                QueueActionForExecution(actionInfo);
            }

            //OnEnter of new state
            var onEnterHandlers = state.GetOnEnterHandlers(newState);
            foreach (var actionInfo in onEnterHandlers)
            {
                QueueActionForExecution(actionInfo);
            }

            state.SetCurrentState(newState);
            
            //OnTransitioned
            var onTransitionedHandlers = state.GetOnTransitionedHandlers(newState);
            foreach (var actionInfo in onTransitionedHandlers)
            {
                QueueActionForExecution(actionInfo);
            }
            
        }
    }
}