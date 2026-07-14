using System;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Contracts
{
    public interface IPhaseService
    {
        GamePhase CurrentPhase { get; }
        string BiomeCalamityId { get; }
        CalamityLevel CurrentLevel { get; }
        float CurrentIntensity { get; }
        bool IsDecayActive { get; }

        event Action<string> OnBiomeCalamityStarted;
        event Action<CalamityLevel> OnLevelChanged;
        event Action<float> OnIntensityChanged;
        event Action OnDecayTriggered;

        void SetPresetBinding(CalamityBinding binding);
        void ProcessStep(StepData step);
        void Tick(float deltaTime, float roundProgress);
        void Reset();
    }
}