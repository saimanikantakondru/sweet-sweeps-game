using System;

namespace SweetSweeps.Core.Contracts
{
    public interface ITimeService
    {
        float RemainingTime { get; }
        float TotalDuration { get; }
        bool IsRunning { get; }

        event Action<float> OnTimeChanged;
        event Action OnTimeExpired;

        void StartTimer();
        void StopTimer();
        void ResetTimer();
        void SetDuration(float seconds);
        void SetTotalDuration(float seconds);
    }
}