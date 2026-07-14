namespace SweetSweeps.Core.Contracts
{
    public interface IGameStateMachine
    {
        IState CurrentState { get; }
        void ChangeState<T>() where T : IState;
    }
}