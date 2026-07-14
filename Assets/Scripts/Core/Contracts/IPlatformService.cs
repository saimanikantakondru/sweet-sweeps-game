namespace SweetSweeps.Core.Contracts
{
    public interface IPlatformService
    {
        void Register(IPlatformMover platform);
        void Clear();
        void FreezeAll();
    }
}
