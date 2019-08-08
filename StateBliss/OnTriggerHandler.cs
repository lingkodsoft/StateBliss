namespace StateBliss
{
    public delegate void OnTriggerHandler<in TContext>(TContext context)
        where TContext : ParentStateContext;
}