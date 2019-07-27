using System.Collections.Generic;

namespace StateBliss
{
    internal class StateTransitionInfo
    {
        public int From;
        public int To;
        public readonly List<(ActionInfo, HandlerType)> Handlers = new List<(ActionInfo, HandlerType)>();
    }
}