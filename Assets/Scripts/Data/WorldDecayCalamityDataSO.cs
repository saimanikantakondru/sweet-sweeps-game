using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "WorldDecayCalamityData", menuName = "SweetSweeps/Calamities/WorldDecay")]
    public class WorldDecayCalamityDataSO : CalamityDataSO
    {
        [Header("Decay")]
        [SerializeField] private float tileRemoveInterval = 0.3f;
        [SerializeField] private int tilesPerWave = 3;
        [SerializeField] private float waveInterval = 1f;

        [Header("Start Position")]
        [SerializeField] private float decayStartOffset = 3f;
        
        public float TileRemoveInterval => tileRemoveInterval;
        public int TilesPerWave => tilesPerWave;
        public float WaveInterval => waveInterval;
        public float DecayStartOffset => decayStartOffset;
    }
}