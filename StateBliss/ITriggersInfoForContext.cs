namespace StateBliss
{
    public interface ITriggersInfoForContext<out TContext>
        where TContext : ParentStateContext
    {
        TContext Context { get; }
    }
}