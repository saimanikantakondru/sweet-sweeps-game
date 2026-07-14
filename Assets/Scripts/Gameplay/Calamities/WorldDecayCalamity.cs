using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Calamities
{
    public class WorldDecayCalamity : ICalamity
    {
        private readonly WorldDecayCalamityEffect _effect;

        public string Id => "world_decay";
        public bool IsActive { get; private set; }

        public event Action<ICalamity> OnExpired;

        public WorldDecayCalamity(WorldDecayCalamityEffect effect)
        {
            _effect = effect;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            _effect.Apply(CalamityLevel.LevelOne, 0f);
            Debug.Log("[WorldDecayCalamity] Activated.");
        }
        
        public void UpdateLevel(CalamityLevel level, float intensity) { }

        public void UpdateIntensity(float intensity) { }

        public void OnStepTick() { }

        public void ForceStop()
        {
            if (!IsActive) return;
            IsActive = false;
            _effect.Remove();
            OnExpired?.Invoke(this);
            Debug.Log("[WorldDecayCalamity] Stopped.");
        }
    }
}