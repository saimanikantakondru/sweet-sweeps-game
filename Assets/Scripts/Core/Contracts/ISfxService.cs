using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface ISfxService
    {
        void PlayOneShot(SoundDefinition sound);
        IAudioHandle PlayLoop(SoundDefinition sound);
        void StopAll();
    }
}
