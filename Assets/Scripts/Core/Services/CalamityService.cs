using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Services
{
    public class CalamityService : ICalamityService
    {
        private readonly Dictionary<string, ICalamity> _registry = new();
        private readonly List<ICalamity> _activeCalamities = new();

        private CalamityLevel _currentLevel = CalamityLevel.None;
        private float _currentIntensity;

        public IReadOnlyList<ICalamity> ActiveCalamities => _activeCalamities;

        public event Action<ICalamity> OnCalamityStarted;
        public event Action<ICalamity> OnCalamityEnded;

        public void RegisterCalamity(ICalamity calamity)
        {
            _registry[calamity.Id] = calamity;
            calamity.OnExpired += OnCalamityExpired;
            Debug.Log($"[CalamityService] Registered: {calamity.Id}");
        }

        public bool HasActive(string calamityId) =>
            _registry.TryGetValue(calamityId, out var c) && c.IsActive;

        public void HandleBiomeCalamityStart(string calamityId, int currentStep)
        {
            if (string.IsNullOrEmpty(calamityId)) return;

            if (!_registry.TryGetValue(calamityId, out var calamity))
            {
                Debug.LogError($"[CalamityService] Unknown biome calamity: {calamityId}");
                return;
            }

            if (calamity.IsActive) return;

            calamity.Activate();
            _activeCalamities.Add(calamity);
            OnCalamityStarted?.Invoke(calamity);
            Debug.Log($"[CalamityService] Biome calamity started: {calamityId} step={currentStep}");
        }

        public void HandleLevelTick(CalamityLevel level)
        {
            _currentLevel = level;
            foreach (var calamity in _activeCalamities)
                calamity.UpdateLevel(level, _currentIntensity);
        }

        public void HandleIntensity(float intensity)
        {
            _currentIntensity = intensity;
            foreach (var calamity in _activeCalamities)
                calamity.UpdateIntensity(intensity);
        }

        public void TriggerDecay()
        {
            const string id = "world_decay";

            if (!_registry.TryGetValue(id, out var decay))
            {
                Debug.LogError("[CalamityService] world_decay not registered.");
                return;
            }

            if (decay.IsActive) return;

            decay.Activate();
            _activeCalamities.Add(decay);
            OnCalamityStarted?.Invoke(decay);
            Debug.Log("[CalamityService] Decay started alongside active calamities.");
        }

        public void OnStepTick(int currentStep)
        {
            for (int i = _activeCalamities.Count - 1; i >= 0; i--)
                _activeCalamities[i].OnStepTick();
        }

        public void WindDown()
        {
            if (_activeCalamities.Count == 0) return;

            for (int i = _activeCalamities.Count - 1; i >= 0; i--)
                _activeCalamities[i].ForceStop();

            _activeCalamities.Clear();
            Debug.Log("[CalamityService] Wound down on round end.");
        }

        public void Reset()
        {
            for (int i = _activeCalamities.Count - 1; i >= 0; i--)
                _activeCalamities[i].ForceStop();

            _activeCalamities.Clear();
            _currentLevel = CalamityLevel.None;
            _currentIntensity = 0f;
            Debug.Log("[CalamityService] Reset.");
        }

        private void OnCalamityExpired(ICalamity calamity)
        {
            _activeCalamities.Remove(calamity);
            OnCalamityEnded?.Invoke(calamity);
            Debug.Log($"[CalamityService] Expired: {calamity.Id}");
        }
    }
}