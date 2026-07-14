namespace SweetSweeps.Core.Contracts
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}