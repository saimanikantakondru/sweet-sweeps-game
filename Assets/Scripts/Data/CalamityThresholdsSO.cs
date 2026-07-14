using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "CalamityThresholds", menuName = "SweetSweeps/Calamities/CalamityThresholds")]
    public class CalamityThresholdsSO : ScriptableObject
    {
        [Header("Time Thresholds (0-1)")]
        [SerializeField] private float levelOneThreshold = 0.5f;
        [SerializeField] private float levelTwoThreshold = 0.75f;

        public float LevelOneThreshold => levelOneThreshold;
        public float LevelTwoThreshold => levelTwoThreshold;
    }
}