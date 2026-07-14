namespace SweetSweeps.Core.Contracts
{
    public interface IAudioHandle
    {
        bool IsPlaying { get; }
        void SetPitch(float pitch);
        void Stop();
    }
}
