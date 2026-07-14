using System;
using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Server.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Services
{
    public class StepService : IStepService, ITickable
    {
        private readonly ICollectibleService _collectibleService;
        private readonly ICalamityService _calamityService;
        private readonly IPhaseService _phaseService;
        private readonly ITimeService _timeService;
        private readonly GameSettingsSO _gameSettings;

        private LevelPresetSO _currentPreset;
        private float _stepInterval = 1f;
        private float _stepTimer;
        private System.Collections.Generic.Queue<StepData> _pendingSteps = new();

        private int _receivedSteps;
        private float _timeSinceLastStep;
        private bool _playbackStarted;
        private bool _stallSignaled;

        public int CurrentStep { get; private set; }
        public int TotalSteps { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<StepData> OnStepReceived;
        public event Action OnAllStepsComplete;
        public event Action OnStreamStalled;

        public StepService(
            ICollectibleService collectibleService,
            ICalamityService calamityService,
            IPhaseService phaseService,
            ITimeService timeService,
            GameSettingsSO gameSettings)
        {
            _collectibleService = collectibleService;
            _calamityService = calamityService;
            _phaseService = phaseService;
            _timeService = timeService;
            _gameSettings = gameSettings;

            _phaseService.OnBiomeCalamityStarted += OnBiomeCalamityStarted;
            _phaseService.OnLevelChanged += OnLevelChanged;
            _phaseService.OnIntensityChanged += OnIntensityChanged;
            _phaseService.OnDecayTriggered += OnDecayTriggered;
        }

        public void StartSession(SessionDataJson session)
        {
            TotalSteps = session.totalSteps;
            CurrentStep = 0;
            _receivedSteps = 0;
            _timeSinceLastStep = 0f;
            _playbackStarted = false;
            _stallSignaled = false;
            IsRunning = true;

            float duration = _gameSettings.CalculateRoundDuration(session.totalSteps);
            _timeService.SetTotalDuration(duration);

            Debug.Log($"[StepService] Session started. TotalSteps={TotalSteps} Duration={duration:F1}s");
        }

        public void ReceiveStep(StepData step)
        {
            _pendingSteps.Enqueue(step);
            _receivedSteps++;
            _timeSinceLastStep = 0f;
        }

        public void SetPreset(LevelPresetSO preset) => _currentPreset = preset;

        public void Stop()
        {
            IsRunning = false;
            Debug.Log("[StepService] Stopped.");
        }

        public void Reset()
        {
            CurrentStep = 0;
            TotalSteps = 0;
            IsRunning = false;
            _receivedSteps = 0;
            _timeSinceLastStep = 0f;
            _playbackStarted = false;
            _stallSignaled = false;
            _stepTimer = 0f;
            _pendingSteps.Clear();
            _phaseService.Reset();
            _calamityService.Reset();
            Debug.Log("[StepService] Reset.");
        }

        public void Tick()
        {
            if (!IsRunning) return;

            float deltaTime = UnityEngine.Time.deltaTime;

            if (DetectStall(deltaTime)) return;

            float roundProgress = TotalSteps > 0
                ? (float)CurrentStep / TotalSteps
                : 0f;

            _phaseService.Tick(deltaTime, roundProgress);

            if (!TryBeginPlayback()) return;

            if (_pendingSteps.Count == 0) return;

            _stepTimer -= deltaTime;
            if (_stepTimer > 0f) return;

            _stepTimer = _stepInterval;
            ProcessStep(_pendingSteps.Dequeue());
        }

        private bool DetectStall(float deltaTime)
        {
            if (_stallSignaled || _receivedSteps >= TotalSteps)
                return false;

            _timeSinceLastStep += deltaTime;
            if (_timeSinceLastStep < _gameSettings.StepStreamStallTimeout)
                return false;

            _stallSignaled = true;
            Debug.LogWarning(
                $"[StepService] Step stream stalled. " +
                $"Received={_receivedSteps}/{TotalSteps} Idle={_timeSinceLastStep:F1}s");
            OnStreamStalled?.Invoke();
            return true;
        }

        private bool TryBeginPlayback()
        {
            if (_playbackStarted) return true;

            bool bufferReady = _pendingSteps.Count >= _gameSettings.StepPrebufferCount;
            bool allArrived = _receivedSteps >= TotalSteps;
            if (!bufferReady && !allArrived) return false;

            _playbackStarted = true;
            _timeService.StartTimer();
            Debug.Log($"[StepService] Playback started. Buffered={_pendingSteps.Count}");
            return true;
        }

        private void ProcessStep(StepData step)
        {
            CurrentStep = step.step;

            _phaseService.ProcessStep(step);

            if (_currentPreset != null)
                _collectibleService.SpawnStep(step, _currentPreset);

            OnStepReceived?.Invoke(step);
            Debug.Log($"[StepService] Step={step.step}");

            if (CurrentStep >= TotalSteps)
            {
                IsRunning = false;
                OnAllStepsComplete?.Invoke();
                Debug.Log("[StepService] All steps complete.");
            }
        }

        private void OnBiomeCalamityStarted(string calamityId)
        {
            _calamityService.HandleBiomeCalamityStart(calamityId, CurrentStep);
        }

        private void OnLevelChanged(CalamityLevel level)
        {
            _calamityService.HandleLevelTick(level);
        }

        private void OnIntensityChanged(float intensity)
        {
            _calamityService.HandleIntensity(intensity);
        }

        private void OnDecayTriggered()
        {
            _calamityService.TriggerDecay();
        }
    }
}