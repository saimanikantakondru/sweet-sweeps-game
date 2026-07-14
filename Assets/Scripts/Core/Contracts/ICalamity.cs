using System;

namespace SweetSweeps.Core.Contracts
{
    public interface ICalamity
    {
        string Id { get; }
        bool IsActive { get; }

        event Action<ICalamity> OnExpired;

        void Activate();
        void UpdateLevel(CalamityLevel level, float intensity);
        void UpdateIntensity(float intensity);
        void OnStepTick();
        void ForceStop();
    }
}