using Cysharp.Threading.Tasks;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Contracts
{
    public interface IGameApiService
    {
        UniTask<InitializeResponse> Initialize(string token, string brand, string game, string currency);
        UniTask<PlayStartResponse> StartRound(string token, string game, float wager);
        UniTask<PlayCompleteResponse> CompleteRound(string token, string game, string gameRound, CollectionReport report);
    }
}