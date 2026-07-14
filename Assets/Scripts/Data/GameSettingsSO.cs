using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "SweetSweeps/GameSettings")]
    public class GameSettingsSO : ScriptableObject
    {
        [Header("Round")]
        [SerializeField] private float roundDuration = 60f;
        [SerializeField] private float stepDurationSeconds = 1f;
        [SerializeField] private float roundEndOffset = 2f;

        [Header("Step Stream")]
        [SerializeField, Tooltip("Steps buffered before playback starts, to cushion network jitter.")]
        private int stepPrebufferCount = 3;
        [SerializeField, Tooltip("Seconds without a new step before the stream is treated as stalled.")]
        private float stepStreamStallTimeout = 3f;

        [Header("Countdown")]
        [SerializeField, Tooltip("Numbers shown before the round starts (e.g. 3 -> 3,2,1).")]
        private int countdownFrom = 3;
        [SerializeField, Tooltip("Seconds each countdown tick (and the final GO) stays on screen. Slightly under 1s feels snappier.")]
        private float countdownStepSeconds = 0.6f;

        [Header("World")]
        [SerializeField] private string[] availableWorldIds = { "World_01" };

        [Header("Hazards")]
        [SerializeField] private int pitDamage = 1;
        
        [Header("Simulation")]
        [SerializeField] private bool useSimulation = true;

#if !UNITY_WEBGL || UNITY_EDITOR
        [SerializeField, Tooltip("Test balance applied when UseSimulation is on. Available in editor and PC/mobile builds; stripped from WebGL (prod).")]
        private float simulationBalance = 5000f;

        public float SimulationBalance => simulationBalance;
#endif

        [Header("Physics Layers")]
        [SerializeField] private LayerMask groundLayer;
        
        [Header("Progress Storage")]
        [SerializeField] private bool useEncryptedStorage = true;

        public float RoundDuration => roundDuration;
        public float StepDurationSeconds => stepDurationSeconds;
        public float RoundEndOffset => roundEndOffset;
        public int StepPrebufferCount => stepPrebufferCount;
        public float StepStreamStallTimeout => stepStreamStallTimeout;
        public int CountdownFrom => countdownFrom;
        public float CountdownStepSeconds => countdownStepSeconds;
        public string[] AvailableWorldIds => availableWorldIds;
        public int PitDamage => pitDamage;
        public bool UseSimulation => useSimulation;
        public LayerMask GroundLayer => groundLayer;
        public bool UseEncryptedStorage => useEncryptedStorage;
        
        public float CalculateRoundDuration(int totalSteps)
        {
            return totalSteps * stepDurationSeconds + roundEndOffset;
        }
    }
}