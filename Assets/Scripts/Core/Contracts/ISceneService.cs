using Cysharp.Threading.Tasks;

namespace SweetSweeps.Core.Contracts
{
    public interface ISceneService
    {
        UniTask LoadBootstrapAsync();
        UniTask LoadMenuAsync();
        UniTask LoadGameplayAsync();
    }
}