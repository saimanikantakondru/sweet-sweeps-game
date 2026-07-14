using System.Collections;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private float defaultPitch = 1f;

        private AudioSource _activeSource;
        private AudioSource _idleSource;
        private AudioSource _layerSource;
        private AudioSource _stingerSource;
        private Coroutine _pitchCoroutine;
        private Coroutine _fadeCoroutine;
        private Coroutine _layerFadeCoroutine;

        private float _baseVolume = 1f;
        private float _basePitch = 1f;
        private float _pitchMultiplier = 1f;
        private float _layerBaseVolume = 1f;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            _activeSource = musicSource;
            _idleSource = CreateMirror(musicSource);
            _layerSource = CreateMirror(musicSource);
            _stingerSource = CreateMirror(musicSource);
            _stingerSource.volume = 1f;
            _pitchMultiplier = defaultPitch;
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            AudioListener.volume = muted ? 0f : 1f;
        }

        public void PlayMusic(SoundDefinition music, float fadeSeconds = 0f)
        {
            if (music == null || !music.IsValid) return;

            AudioClip clip = music.PickClip();
            if (_activeSource.isPlaying && _activeSource.clip == clip) return;

            _baseVolume = music.PickVolume();
            _basePitch = music.PickPitch();

            if (fadeSeconds <= 0f)
            {
                PlayImmediate(clip, music.Loop);
                return;
            }

            StartCrossfade(clip, music.Loop, fadeSeconds);
        }

        public void PlayMusicLayer(SoundDefinition music, float fadeSeconds = 0f)
        {
            if (music == null || !music.IsValid) return;

            _layerBaseVolume = music.PickVolume();
            _layerSource.clip = music.PickClip();
            _layerSource.loop = music.Loop;
            _layerSource.pitch = music.PickPitch();

            if (!_layerSource.isPlaying) _layerSource.volume = 0f;
            _layerSource.Play();

            FadeLayerTo(_layerBaseVolume, fadeSeconds);
        }

        public void StopMusicLayer(float fadeSeconds = 0f)
        {
            if (_activeSource == null) return;
            
            if (fadeSeconds <= 0f)
            {
                if (_layerFadeCoroutine != null) StopCoroutine(_layerFadeCoroutine);
                _layerSource.Stop();
                _layerSource.volume = 0f;
                return;
            }

            FadeLayerTo(0f, fadeSeconds, stopAtEnd: true);
        }

        public void PlayStinger(SoundDefinition stinger)
        {
            if (stinger == null || !stinger.IsValid) return;

            _stingerSource.pitch = stinger.PickPitch();
            _stingerSource.PlayOneShot(stinger.PickClip(), stinger.PickVolume());
        }

        public void StopMusic(float fadeSeconds = 0f)
        {
            if (_activeSource == null) return;

            if (fadeSeconds <= 0f)
            {
                _activeSource.Stop();
                _idleSource.Stop();
                return;
            }

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutRoutine(fadeSeconds));
        }

        public void SetMusicPitch(float multiplier, float transitionDuration = 0.5f)
        {
            if (!CanPlayCoroutine() || transitionDuration <= 0f)
            {
                ApplyMusicPitch(multiplier);
                return;
            }

            if (_pitchCoroutine != null) StopCoroutine(_pitchCoroutine);
            _pitchCoroutine = StartCoroutine(PitchRoutine(multiplier, transitionDuration));
        }

        private void ApplyMusicPitch(float multiplier)
        {
            _pitchMultiplier = multiplier;
            if (_activeSource != null)
                _activeSource.pitch = _basePitch * _pitchMultiplier;
        }

        private bool CanPlayCoroutine() => this != null && isActiveAndEnabled;

        public void ResetMusicPitch(float transitionDuration = 0.5f)
        {
            SetMusicPitch(defaultPitch, transitionDuration);
        }

        private void PlayImmediate(AudioClip clip, bool loop)
        {
            _activeSource.clip = clip;
            _activeSource.loop = loop;
            _activeSource.volume = _baseVolume;
            _activeSource.pitch = _basePitch * _pitchMultiplier;
            _activeSource.Play();
        }

        private void StartCrossfade(AudioClip clip, bool loop, float duration)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            _idleSource.clip = clip;
            _idleSource.loop = loop;
            _idleSource.pitch = _basePitch * _pitchMultiplier;
            _idleSource.volume = 0f;
            _idleSource.Play();

            _fadeCoroutine = StartCoroutine(CrossfadeRoutine(duration));
        }

        private void FadeLayerTo(float targetVolume, float duration, bool stopAtEnd = false)
        {
            if (_layerFadeCoroutine != null) StopCoroutine(_layerFadeCoroutine);

            if (duration <= 0f)
            {
                _layerSource.volume = targetVolume;
                if (stopAtEnd) _layerSource.Stop();
                return;
            }

            _layerFadeCoroutine = StartCoroutine(FadeLayerRoutine(targetVolume, duration, stopAtEnd));
        }

        private IEnumerator FadeLayerRoutine(float targetVolume, float duration, bool stopAtEnd)
        {
            float startVolume = _layerSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _layerSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            _layerSource.volume = targetVolume;
            if (stopAtEnd) _layerSource.Stop();
            _layerFadeCoroutine = null;
        }

        private IEnumerator CrossfadeRoutine(float duration)
        {
            AudioSource from = _activeSource;
            AudioSource to = _idleSource;
            float fromStart = from.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                from.volume = Mathf.Lerp(fromStart, 0f, t);
                to.volume = Mathf.Lerp(0f, _baseVolume, t);
                yield return null;
            }

            from.volume = 0f;
            to.volume = _baseVolume;
            from.Stop();

            _activeSource = to;
            _idleSource = from;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float startVolume = _activeSource.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _activeSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _activeSource.Stop();
            _activeSource.volume = _baseVolume;
            _fadeCoroutine = null;
        }

        private IEnumerator PitchRoutine(float targetMultiplier, float duration)
        {
            float start = _pitchMultiplier;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _pitchMultiplier = Mathf.Lerp(start, targetMultiplier, elapsed / duration);
                _activeSource.pitch = _basePitch * _pitchMultiplier;
                yield return null;
            }

            _pitchMultiplier = targetMultiplier;
            _activeSource.pitch = _basePitch * _pitchMultiplier;
            _pitchCoroutine = null;
        }

        private AudioSource CreateMirror(AudioSource template)
        {
            var mirror = gameObject.AddComponent<AudioSource>();
            mirror.outputAudioMixerGroup = template.outputAudioMixerGroup;
            mirror.playOnAwake = false;
            mirror.spatialBlend = template.spatialBlend;
            mirror.volume = 0f;
            return mirror;
        }
    }
}
