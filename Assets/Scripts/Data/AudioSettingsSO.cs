using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "SweetSweeps/Audio/AudioSettings")]
    public class AudioSettingsSO : ScriptableObject
    {
        [Header("Music by default")]
        [SerializeField] private SoundDefinition gameplayMusic;

        [Header("Stingers")]
        [SerializeField] private SoundDefinition gameStartJingle;
        [SerializeField] private SoundDefinition gameOverJingle;

        [Header("Footsteps")]
        [SerializeField, Min(0.05f), Tooltip("Seconds between footstep one-shots while the player is walking.")]
        private float walkStepInterval = 0.4f;

        [Header("Calamity Pitch (multiplier on track base pitch)")]
        [SerializeField] private float pitchLevelOne = 0.85f;
        [SerializeField] private float pitchLevelTwo = 0.7f;
        [SerializeField] private float pitchTransitionDuration = 0.6f;

        public SoundDefinition GameplayMusic => gameplayMusic;
        public SoundDefinition GameStartJingle => gameStartJingle;
        public SoundDefinition GameOverJingle => gameOverJingle;
        public float WalkStepInterval => walkStepInterval;
        public float PitchLevelOne => pitchLevelOne;
        public float PitchLevelTwo => pitchLevelTwo;
        public float PitchTransitionDuration => pitchTransitionDuration;
    }
}