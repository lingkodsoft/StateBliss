namespace StateBliss
{
    public interface IStateMachineManager
    {
        void Register(IState state);
        void ChamgeState(IState state, object newState);
    }
}