using Cysharp.Threading.Tasks;

namespace SweetSweeps.Core.Contracts
{
    public interface IMessageBoxService
    {
        UniTask ShowAsync(string title, string message, string confirmLabel = "OK");
    }
}