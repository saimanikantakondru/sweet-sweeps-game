using System;
using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class TimeService : ITimeService, ITickable
    {
        private readonly GameSettingsSO _settings;

        public float RemainingTime { get; private set; }
        public float TotalDuration { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<float> OnTimeChanged;
        public event Action OnTimeExpired;

        public TimeService(GameSettingsSO settings)
        {
            _settings = settings;
            TotalDuration = settings.RoundDuration;
            RemainingTime = TotalDuration;
        }
        
        public void SetDuration(float seconds)
        {
            TotalDuration = seconds;
            RemainingTime = seconds;
            OnTimeChanged?.Invoke(RemainingTime);
        }

        public void SetTotalDuration(float seconds)
        {
            float elapsed = Mathf.Max(0f, TotalDuration - RemainingTime);
            TotalDuration = seconds;
            RemainingTime = Mathf.Clamp(seconds - elapsed, 0f, seconds);
            OnTimeChanged?.Invoke(RemainingTime);
        }

        public void StartTimer()
        {
            IsRunning = true;
        }

        public void StopTimer()
        {
            IsRunning = false;
        }

        public void ResetTimer()
        {
            RemainingTime = TotalDuration;
            IsRunning = false;
        }

        public void Tick()
        {
            if (!IsRunning) return;

            RemainingTime -= Time.deltaTime;
            OnTimeChanged?.Invoke(RemainingTime);

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRunning = false;
                OnTimeExpired?.Invoke();
            }
        }
    }
}