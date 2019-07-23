using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace StateBliss
{
    public class StateActionProcessor
    {

        private BlockingCollection<ActionInfo> _actionTriggerQueue = new BlockingCollection<ActionInfo>();
        
//        private ConcurrentQueue<string> _actionTriggerQueue = new ConcurrentQueue<string>();
        
        private async Task ProcessActionQueue()
        {
            //_actionTriggerQueue.

            while (true)
            {
                foreach (var actionInfo in _actionTriggerQueue.GetConsumingEnumerable())
                {
                    actionInfo.Execute();
                }    
            }
            
        }
    }
}