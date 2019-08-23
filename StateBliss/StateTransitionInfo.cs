using System.Collections.Generic;

namespace StateBliss
{
    public class StateTransitionInfo
    {
        public int From;
        public int To;
        public readonly List<(ActionInfo, HandlerType)> Handlers = new List<(ActionInfo, HandlerType)>();
    }
}