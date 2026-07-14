using UnityEngine;
using UnityEngine.Audio;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "AudioMixerConfig", menuName = "SweetSweeps/Audio/Mixer Config")]
    public class AudioMixerConfigSO : ScriptableObject
    {
        public const string MusicVolumeParam = "MusicVolume";
        public const string SfxVolumeParam = "SfxVolume";

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        public AudioMixer Mixer => mixer;
        public AudioMixerGroup MusicGroup => musicGroup;
        public AudioMixerGroup SfxGroup => sfxGroup;
    }
}
