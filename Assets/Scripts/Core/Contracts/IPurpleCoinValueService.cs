using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Contracts
{
    public interface IPurpleCoinValueService
    {
        void SetNormalValues(float[] values);
        void SetCalamityValues(float[] values);
        bool TryGetNextValue(GamePhase phase, out float value);
        void Reset();
    }
}
