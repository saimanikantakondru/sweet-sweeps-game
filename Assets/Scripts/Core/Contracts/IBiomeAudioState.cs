using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface IBiomeAudioState
    {
        Surface CurrentSurface { get; }
        string CurrentBiomeId { get; }
    }
}
