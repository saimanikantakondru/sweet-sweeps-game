using Cysharp.Threading.Tasks;

namespace SweetSweeps.Server.Contracts
{
    public interface ISessionInitializer
    {
        string SelectedCurrency { get; }

        void ResolveToken();
        void SetCurrency(string currency);
        UniTask InitializeAsync();
    }
}
