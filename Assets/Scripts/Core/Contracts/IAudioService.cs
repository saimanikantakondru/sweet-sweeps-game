using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface IAudioService
    {
        bool IsMuted { get; }

        void PlayMusic(SoundDefinition music, float fadeSeconds = 0f);
        void PlayMusicLayer(SoundDefinition music, float fadeSeconds = 0f);
        void StopMusicLayer(float fadeSeconds = 0f);
        void PlayStinger(SoundDefinition stinger);
        void StopMusic(float fadeSeconds = 0f);
        void SetMusicPitch(float multiplier, float transitionDuration = 0.5f);
        void ResetMusicPitch(float transitionDuration = 0.5f);
        void SetMuted(bool muted);
    }
}