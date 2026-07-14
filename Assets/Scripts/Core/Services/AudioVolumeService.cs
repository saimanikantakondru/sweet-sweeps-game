using UnityEngine;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class AudioVolumeService : IAudioVolumeService, IStartable
    {
        private const string MusicPrefKey = "audio_music_volume";
        private const string SfxPrefKey = "audio_sfx_volume";
        private const float MinDb = -80f;

        private readonly AudioMixerConfigSO _config;

        public float MusicVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;

        public AudioVolumeService(AudioMixerConfigSO config)
        {
            _config = config;
        }

        public void Start()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicPrefKey, 1f);
            SfxVolume = PlayerPrefs.GetFloat(SfxPrefKey, 1f);

            Apply(AudioMixerConfigSO.MusicVolumeParam, MusicVolume);
            Apply(AudioMixerConfigSO.SfxVolumeParam, SfxVolume);
        }

        public void SetMusicVolume(float normalized)
        {
            MusicVolume = Mathf.Clamp01(normalized);
            PlayerPrefs.SetFloat(MusicPrefKey, MusicVolume);
            Apply(AudioMixerConfigSO.MusicVolumeParam, MusicVolume);
        }

        public void SetSfxVolume(float normalized)
        {
            SfxVolume = Mathf.Clamp01(normalized);
            PlayerPrefs.SetFloat(SfxPrefKey, SfxVolume);
            Apply(AudioMixerConfigSO.SfxVolumeParam, SfxVolume);
        }

        private void Apply(string param, float normalized)
        {
            float db = normalized <= 0.0001f ? MinDb : Mathf.Log10(normalized) * 20f;
            _config.Mixer.SetFloat(param, db);
        }
    }
}
