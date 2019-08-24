using System;
using System.Collections.Generic;

namespace StateBliss
{
    public interface IStateDefinition
    {
        void Define();
        IReadOnlyList<StateTransitionInfo> Transitions { get; }
        IReadOnlyList<int> DisabledSameStateTransitions { get; }
        Type EnumType { get; }
    }
}