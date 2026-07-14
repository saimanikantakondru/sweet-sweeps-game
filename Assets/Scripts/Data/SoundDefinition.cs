using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "snd_", menuName = "SweetSweeps/Audio/Sound Definition")]
    public class SoundDefinition : ScriptableObject
    {
        [SerializeField, Required] private AudioClip[] clips;
        [SerializeField] private bool loop;
        [SerializeField, MinMaxSlider(0f, 1f, true)] private Vector2 volume = Vector2.one;
        [SerializeField, MinMaxSlider(0.1f, 2f, true)] private Vector2 pitch = Vector2.one;
        [SerializeField, Range(0, 256)] private int priority = 128;

        [Title("Layers (played simultaneously)")]
        [SerializeField] private SoundDefinition[] layers;

        private int _lastIndex = -1;

        public bool Loop => loop;
        public int Priority => priority;
        public bool IsValid => clips != null && clips.Length > 0;
        public IReadOnlyList<SoundDefinition> Layers => layers;

        public AudioClip PickClip()
        {
            if (!IsValid) return null;
            if (clips.Length == 1) return clips[0];

            int index;
            do { index = Random.Range(0, clips.Length); }
            while (index == _lastIndex);

            _lastIndex = index;
            return clips[index];
        }

        public float PickVolume() => Random.Range(volume.x, volume.y);
        public float PickPitch() => Random.Range(pitch.x, pitch.y);
    }
}
