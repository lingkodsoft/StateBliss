namespace StateBliss.SampleApi
{
    public interface IStateDefinitionHandler<TContext> : IStateDefinitionHandler where TContext : StateContext
    {
        IGuardsInfo<TContext> GetHandler();
    }

    public interface IStateDefinitionHandler
    {
    }
}