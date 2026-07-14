using UnityEngine;
using SweetSweeps.Data;
using Sirenix.OdinInspector;

namespace SweetSweeps.Gameplay.Level
{
    [RequireComponent(typeof(PlatformMover))]
    public class PlatformAudioEmitter : MonoBehaviour
    {
        private const float MovingThresholdSqr = 0.0001f;

        [SerializeField, Required] private SoundDefinition sound;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;
        [SerializeField] private float maxDistance = 20f;

        private Rigidbody2D _rigidbody;
        private AudioSource _source;
        private bool _playing;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _source = gameObject.AddComponent<AudioSource>();
            ConfigureSource();
        }

        private void Update()
        {
            bool moving = _rigidbody.linearVelocity.sqrMagnitude > MovingThresholdSqr;
            if (moving == _playing) return;

            _playing = moving;

            if (moving) _source.Play();
            else _source.Stop();
        }

        private void ConfigureSource()
        {
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = spatialBlend;
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.maxDistance = maxDistance;

            if (sound == null || !sound.IsValid) return;

            _source.clip = sound.PickClip();
            _source.volume = sound.PickVolume();
            _source.pitch = sound.PickPitch();
        }
    }
}
