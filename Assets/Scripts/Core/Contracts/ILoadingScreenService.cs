namespace SweetSweeps.Core.Contracts
{
    public interface ILoadingScreenService
    {
        void Show(string message = null);
        void Hide();
    }
}
