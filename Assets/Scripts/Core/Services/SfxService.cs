using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class SfxService : ISfxService
    {
        private readonly AudioSourcePool _pool;
        private readonly List<AudioHandle> _activeLoops = new();

        public SfxService(AudioSourcePool pool)
        {
            _pool = pool;
        }

        public void PlayOneShot(SoundDefinition sound)
        {
            if (sound == null) return;

            PlayClip(sound);

            IReadOnlyList<SoundDefinition> layers = sound.Layers;
            if (layers == null) return;

            for (int i = 0; i < layers.Count; i++)
                PlayClip(layers[i]);
        }

        private void PlayClip(SoundDefinition sound)
        {
            if (sound == null || !sound.IsValid) return;

            AudioSource source = _pool.Rent();
            Configure(source, sound, loop: false);
            source.Play();

            _pool.TrackOneShot(source);
        }

        public IAudioHandle PlayLoop(SoundDefinition sound)
        {
            if (sound == null || !sound.IsValid) return NullAudioHandle.Instance;

            AudioSource source = _pool.Rent();
            Configure(source, sound, loop: true);
            source.Play();

            var handle = new AudioHandle(source, _pool, h => _activeLoops.Remove(h));
            _activeLoops.Add(handle);
            return handle;
        }

        public void StopAll()
        {
            for (int i = _activeLoops.Count - 1; i >= 0; i--)
                _activeLoops[i].Stop();

            _activeLoops.Clear();
            _pool.ReturnAllOneShots();
        }

        private static void Configure(AudioSource source, SoundDefinition sound, bool loop)
        {
            source.clip = sound.PickClip();
            source.volume = sound.PickVolume();
            source.pitch = sound.PickPitch();
            source.priority = sound.Priority;
            source.loop = loop;
        }
    }
}
