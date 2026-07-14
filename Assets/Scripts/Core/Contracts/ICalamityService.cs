using System;
using System.Collections.Generic;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Contracts
{
    public interface ICalamityService
    {
        IReadOnlyList<ICalamity> ActiveCalamities { get; }
        bool HasActive(string calamityId);

        event Action<ICalamity> OnCalamityStarted;
        event Action<ICalamity> OnCalamityEnded;

        void RegisterCalamity(ICalamity calamity);
        void HandleBiomeCalamityStart(string calamityId, int currentStep);
        void HandleLevelTick(CalamityLevel level);
        void HandleIntensity(float intensity);
        void TriggerDecay();
        void OnStepTick(int currentStep);
        void WindDown();
        void Reset();
    }
}