using System;
using UnityEngine;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface ICollectibleService
    {
        event Action<CollectibleDataSO> OnCollected;
        event Action<CoinRevealInfo> OnPurpleCoinValueRevealed;

        void RegisterCollected(CollectibleDataSO data, Vector3 worldPosition);
        void SpawnStep(StepData step, LevelPresetSO preset);
        void DespawnAll();
    }
}