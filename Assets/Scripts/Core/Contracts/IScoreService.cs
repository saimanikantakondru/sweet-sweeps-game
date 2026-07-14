using System;

namespace SweetSweeps.Core.Contracts
{
    public interface IScoreService
    {
        float CurrentValue { get; }
        int GoldCoins { get; }
        int SsCoins { get; }

        event Action<float> OnScoreChanged;
        event Action<int> OnSsCoinsChanged;

        void SetWager(float wager);
        void RegisterGoldCoin();
        void RegisterSourCandy();
        void AddSsCoins(int amount);
        void ResetScore();
    }
}
