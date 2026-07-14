using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class BiomeAudioState : IBiomeAudioState
    {
        public Surface CurrentSurface { get; private set; } = Surface.Grass;
        public string CurrentBiomeId { get; private set; } = string.Empty;

        public void SetBiome(string biomeId, Surface surface)
        {
            CurrentBiomeId = biomeId;
            CurrentSurface = surface;
        }

        public void Reset()
        {
            CurrentBiomeId = string.Empty;
            CurrentSurface = Surface.Grass;
        }
    }
}
