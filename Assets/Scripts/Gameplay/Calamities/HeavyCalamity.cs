using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Calamities
{
    public class HeavyCalamity : ICalamity
    {
        private readonly IReadOnlyList<ICalamityEffect> _effects;

        public string Id => "heavy";
        public bool IsActive { get; private set; }

        public event Action<ICalamity> OnExpired;

        public HeavyCalamity(IReadOnlyList<ICalamityEffect> effects)
        {
            _effects = effects;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;

            for (int i = 0; i < _effects.Count; i++)
                _effects[i].Apply(CalamityLevel.LevelOne, 0f);

            Debug.Log("[HeavyCalamity] Activated.");
        }

        public void UpdateLevel(CalamityLevel level, float intensity)
        {
            if (!IsActive) return;

            if (level == CalamityLevel.None)
            {
                ForceStop();
                return;
            }

            for (int i = 0; i < _effects.Count; i++)
                _effects[i].Apply(level, intensity);
        }

        public void UpdateIntensity(float intensity)
        {
            if (!IsActive) return;

            for (int i = 0; i < _effects.Count; i++)
                if (_effects[i] is ICalamityIntensityListener listener)
                    listener.OnIntensity(intensity);
        }

        public void OnStepTick() { }

        public void ForceStop()
        {
            if (!IsActive) return;
            IsActive = false;

            for (int i = 0; i < _effects.Count; i++)
                _effects[i].Remove();

            OnExpired?.Invoke(this);
            Debug.Log("[HeavyCalamity] Stopped.");
        }
    }
}
