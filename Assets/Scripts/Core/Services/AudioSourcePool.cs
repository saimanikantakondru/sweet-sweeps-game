using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using VContainer.Unity;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Services
{
    public class AudioSourcePool : ITickable, IDisposable
    {
        private const int PrewarmCount = 16;
        private const float MaxOneShotLifetime = 30f;

        private readonly Transform _root;
        private readonly AudioMixerGroup _sfxGroup;
        private readonly Stack<AudioSource> _free = new();
        private readonly List<ActiveVoice> _active = new();

        public AudioSourcePool(AudioMixerConfigSO config)
        {
            _sfxGroup = config != null ? config.SfxGroup : null;

            var go = new GameObject("[SfxPool]");
            _root = go.transform;

            for (int i = 0; i < PrewarmCount; i++)
                _free.Push(CreateSource());
        }

        public AudioSource Rent()
        {
            AudioSource source = _free.Count > 0 ? _free.Pop() : CreateSource();
            source.gameObject.SetActive(true);
            return source;
        }

        public void TrackOneShot(AudioSource source)
        {
            _active.Add(new ActiveVoice(source, Time.unscaledTime + MaxOneShotLifetime));
        }

        public void Return(AudioSource source)
        {
            ResetSource(source);
            source.gameObject.SetActive(false);
            _free.Push(source);
        }

        public void Tick()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                ActiveVoice voice = _active[i];

                if (voice.Source.isPlaying)
                {
                    if (!voice.Started)
                    {
                        voice.Started = true;
                        _active[i] = voice;
                    }
                    continue;
                }

                if (voice.Started || Time.unscaledTime >= voice.SafetyDueTime)
                {
                    Return(voice.Source);
                    _active.RemoveAt(i);
                }
            }
        }

        public void ReturnAllOneShots()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                Return(_active[i].Source);

            _active.Clear();
        }

        public void Dispose()
        {
            if (_root != null)
                UnityEngine.Object.Destroy(_root.gameObject);
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject("SfxVoice");
            go.transform.SetParent(_root);
            go.SetActive(false);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = _sfxGroup;
            return source;
        }

        private static void ResetSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
            source.pitch = 1f;
        }

        private struct ActiveVoice
        {
            public readonly AudioSource Source;
            public readonly float SafetyDueTime;
            public bool Started;

            public ActiveVoice(AudioSource source, float safetyDueTime)
            {
                Source = source;
                SafetyDueTime = safetyDueTime;
                Started = false;
            }
        }
    }
}
