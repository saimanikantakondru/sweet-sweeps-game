using System;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Contracts
{
    public interface IGameSessionService
    {
        string Token { get; }
        string Game { get; }
        string SessionUid { get; }
        string GamePlayUid { get; }
        string GameRoundUid { get; }
        WalletData CurrentWallet { get; }
        string ActiveCurrency { get; }
        PlayCompleteData LastRoundResult { get; }
        PlayStartData CurrentRoundData { get; }
        UnfinishedGameData[] UnfinishedGames { get; }
        GameSettings ServerGameSettings { get; }

        bool IsAuthenticated { get; }
        bool HasUnfinishedGame { get; }

        event Action OnWalletsChanged;
        event Action OnAuthenticated;
        event Action OnSessionInitialized;
        event Action<PlayCompleteData> OnRoundResultReceived;
        event Action OnRoundResultFailed;

        WalletData GetWallet(string currency);

        void SetToken(string token);
        void SetGame(string game);
        void ApplyInitializeResponse(InitializeResponse response);
        void ApplyStartResponse(PlayStartResponse response);
        void ApplyCompleteResponse(PlayCompleteResponse response);
        void NotifyRoundResultFailed();
        void Clear();

#if !UNITY_WEBGL || UNITY_EDITOR
        void DebugSetBalance(float balance);
#endif
    }
}