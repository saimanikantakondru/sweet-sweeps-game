using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public sealed class NullAudioHandle : IAudioHandle
    {
        public static readonly NullAudioHandle Instance = new();

        private NullAudioHandle() { }

        public bool IsPlaying => false;
        public void SetPitch(float pitch) { }
        public void Stop() { }
    }
}
