using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Services
{
    public class PhaseService : IPhaseService
    {
        private readonly CalamityThresholdsSO _thresholds;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Normal;
        public string BiomeCalamityId { get; private set; } = string.Empty;
        public CalamityLevel CurrentLevel { get; private set; } = CalamityLevel.None;
        public float CurrentIntensity { get; private set; }
        public bool IsDecayActive { get; private set; }

        public event Action<string> OnBiomeCalamityStarted;
        public event Action<CalamityLevel> OnLevelChanged;
        public event Action<float> OnIntensityChanged;
        public event Action OnDecayTriggered;

        private const float IntensityEpsilon = 0.001f;

        private CalamityBinding _binding;
        private CalamityLevel _lastEmittedLevel = CalamityLevel.None;
        private float _lastEmittedIntensity = -1f;
        private bool _biomeCalamityStarted;
        private bool _decayFired;

        public PhaseService(CalamityThresholdsSO thresholds)
        {
            _thresholds = thresholds;
        }

        public void SetPresetBinding(CalamityBinding binding)
        {
            _binding = binding;
            BiomeCalamityId = binding?.BiomeCalamityId ?? string.Empty;
            Debug.Log($"[PhaseService] Binding set. BiomeCalamity={BiomeCalamityId}");
        }

        public void ProcessStep(StepData step)
        {
            GamePhase incoming = ParsePhase(step.phase);
            if (incoming == GamePhase.Calamity && !_decayFired)
            {
                _decayFired = true;
                IsDecayActive = true;
                CurrentPhase = GamePhase.Calamity;
                Debug.Log($"[PhaseService] Decay triggered by step phase. Step={step.step}");
                OnDecayTriggered?.Invoke();
            }
        }

        public void Tick(float deltaTime, float roundProgress)
        {
            if (string.IsNullOrEmpty(BiomeCalamityId)) return;

            CalamityLevel newLevel = ResolveLevel(roundProgress);
            if (!_biomeCalamityStarted && newLevel >= CalamityLevel.LevelOne)
            {
                _biomeCalamityStarted = true;
                Debug.Log($"[PhaseService] Biome calamity started: {BiomeCalamityId} at {roundProgress:P0}");
                OnBiomeCalamityStarted?.Invoke(BiomeCalamityId);
            }

            if (!_biomeCalamityStarted) return;

            CurrentLevel = newLevel;
            if (newLevel != _lastEmittedLevel)
            {
                _lastEmittedLevel = newLevel;
                Debug.Log($"[PhaseService] Level={newLevel} progress={roundProgress:P0}");
                OnLevelChanged?.Invoke(newLevel);
            }

            float intensity = ResolveIntensity(roundProgress);
            CurrentIntensity = intensity;
            if (Mathf.Abs(intensity - _lastEmittedIntensity) >= IntensityEpsilon)
            {
                _lastEmittedIntensity = intensity;
                OnIntensityChanged?.Invoke(intensity);
            }
        }

        public void Reset()
        {
            CurrentPhase = GamePhase.Normal;
            CurrentLevel = CalamityLevel.None;
            CurrentIntensity = 0f;
            _lastEmittedLevel = CalamityLevel.None;
            _lastEmittedIntensity = -1f;
            _biomeCalamityStarted = false;
            IsDecayActive = false;
            _decayFired = false;
            Debug.Log("[PhaseService] Reset.");
        }

        private CalamityLevel ResolveLevel(float progress)
        {
            if (progress >= _thresholds.LevelTwoThreshold) return CalamityLevel.LevelTwo;
            if (progress >= _thresholds.LevelOneThreshold) return CalamityLevel.LevelOne;
            return CalamityLevel.None;
        }

        private float ResolveIntensity(float progress)
        {
            return Mathf.Clamp01(
                Mathf.InverseLerp(_thresholds.LevelOneThreshold, 1f, progress));
        }

        private static GamePhase ParsePhase(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return GamePhase.Normal;
            return raw.ToLowerInvariant() == "calamity"
                ? GamePhase.Calamity
                : GamePhase.Normal;
        }
    }
}