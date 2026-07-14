using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core
{
    public class AudioCoordinator
    {
        private readonly IAudioService _audioService;
        private readonly ILevelService _levelService;
        private readonly AudioSettingsSO _settings;

        public AudioCoordinator(IAudioService audioService, ILevelService levelService, AudioSettingsSO settings)
        {
            _audioService = audioService;
            _levelService = levelService;
            _settings = settings;
        }

        public void OnRoundStarted()
        {
            _audioService.PlayMusic(ResolveGameplayMusic());
            _audioService.PlayStinger(_settings.GameStartJingle);
        }

        private SoundDefinition ResolveGameplayMusic()
        {
            SoundDefinition worldMusic = _levelService.CurrentPreset?.GameplayMusic;
            return worldMusic != null && worldMusic.IsValid
                ? worldMusic
                : _settings.GameplayMusic;
        }

        public void OnRoundEnded()
        {
            _audioService.StopMusic();
            _audioService.ResetMusicPitch();
        }

        public void OnGameOver()
        {
            _audioService.PlayStinger(_settings.GameOverJingle);
        }

        public void OnBiomeCalamityStarted(string calamityId)
        {
            _audioService.SetMusicPitch(_settings.PitchLevelOne, _settings.PitchTransitionDuration);
        }

        public void OnCalamityLevelChanged(CalamityLevel level)
        {
            switch (level)
            {
                case CalamityLevel.LevelOne:
                    _audioService.SetMusicPitch(_settings.PitchLevelOne, _settings.PitchTransitionDuration);
                    break;

                case CalamityLevel.LevelTwo:
                    _audioService.SetMusicPitch(_settings.PitchLevelTwo, _settings.PitchTransitionDuration);
                    break;
            }
        }
    }
}