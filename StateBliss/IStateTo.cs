namespace StateBliss
{
    public interface IStateTo
    {
        IStateTransitionBuilder To(StatusEnum clicked);
    }
}