using System;

namespace StateBliss.SampleApi
{
    public interface IStateDefinition
    {
        Type EnumType { get; }
        State DefineState(Guid id);
    }
}