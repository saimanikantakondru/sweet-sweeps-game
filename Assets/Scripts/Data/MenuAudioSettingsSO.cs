using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "MenuAudioSettings", menuName = "SweetSweeps/MenuAudioSettings")]
    public class MenuAudioSettingsSO : ScriptableObject
    {
        [Header("Music")]
        [SerializeField] private SoundDefinition defaultMusic;

        public SoundDefinition DefaultMusic => defaultMusic;
    }
}
