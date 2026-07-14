namespace SweetSweeps.Core.Contracts
{
    public interface IAudioVolumeService
    {
        float MusicVolume { get; }
        float SfxVolume { get; }

        void SetMusicVolume(float normalized);
        void SetSfxVolume(float normalized);
    }
}
