using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class UiAudioService : IUiAudioService
    {
        private readonly SoundDefinition _click;
        private readonly AudioSource _source;

        public UiAudioService(AudioMixerConfigSO mixerConfig, SoundDefinition clickSound)
        {
            _click = clickSound;

            var go = new GameObject("[UiAudio]");
            Object.DontDestroyOnLoad(go);

            _source = go.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.outputAudioMixerGroup = mixerConfig != null ? mixerConfig.SfxGroup : null;
        }

        public void PlayClick()
        {
            if (_click == null || !_click.IsValid) return;

            _source.pitch = _click.PickPitch();
            _source.PlayOneShot(_click.PickClip(), _click.PickVolume());
        }
    }
}
