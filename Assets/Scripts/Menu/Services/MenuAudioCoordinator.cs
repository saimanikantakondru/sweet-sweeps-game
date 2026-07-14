using System;
using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Menu.Services
{
    public class MenuAudioCoordinator : IStartable, IDisposable
    {
        private readonly IAudioService _audioService;
        private readonly MenuAudioSettingsSO _settings;

        public MenuAudioCoordinator(IAudioService audioService, MenuAudioSettingsSO settings)
        {
            _audioService = audioService;
            _settings = settings;
        }

        public void Start()
        {
            _audioService.StopMusicLayer();
            _audioService.ResetMusicPitch(0f);

            if (_settings == null)
            {
                Debug.LogError("[MenuAudioCoordinator] MenuAudioSettings is not assigned.");
                return;
            }

            if (_settings.DefaultMusic == null)
            {
                Debug.LogWarning("[MenuAudioCoordinator] DefaultMusic is empty — skipping playback.");
                return;
            }

            _audioService.PlayMusic(_settings.DefaultMusic);
        }

        public void Dispose()
        {
            _audioService.StopMusic();
        }
    }
}
