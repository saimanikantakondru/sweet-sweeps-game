using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface ICalamityVisualPresenter
    {
        void ApplyVisual(CalamityVisualProfileSO profile, CalamityLevel level, float intensity);
        void UpdateIntensity(float intensity);
        void ClearVisual(CalamityVisualProfileSO profile);
    }
}
