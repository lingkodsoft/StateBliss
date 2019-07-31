using System;

namespace StateBliss
{
    public delegate State StateFactory(Type stateEnumType, Guid id);
}