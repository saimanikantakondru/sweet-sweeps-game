using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class ScoreService : IScoreService
    {
        private const float GoldCoinValueRate = 0.25f;
        private const float SourPenaltyFactor = 0.75f;

        private int _goldCoins;
        private int _sourHits;
        private float _wager;

        public float CurrentValue =>
            _goldCoins * Mathf.Pow(SourPenaltyFactor, _sourHits) * GoldCoinValueRate * _wager;

        public int GoldCoins => _goldCoins;
        public int SsCoins { get; private set; }

        public event Action<float> OnScoreChanged;
        public event Action<int> OnSsCoinsChanged;

        public void SetWager(float wager)
        {
            _wager = Mathf.Max(0f, wager);
            OnScoreChanged?.Invoke(CurrentValue);
        }

        public void RegisterGoldCoin()
        {
            _goldCoins++;
            OnScoreChanged?.Invoke(CurrentValue);
        }

        public void RegisterSourCandy()
        {
            _sourHits++;
            OnScoreChanged?.Invoke(CurrentValue);
        }

        public void AddSsCoins(int amount)
        {
            if (amount <= 0) return;
            SsCoins += amount;
            OnSsCoinsChanged?.Invoke(SsCoins);
        }

        public void ResetScore()
        {
            _goldCoins = 0;
            _sourHits = 0;
            SsCoins = 0;
            OnScoreChanged?.Invoke(CurrentValue);
            OnSsCoinsChanged?.Invoke(SsCoins);
        }
    }
}
