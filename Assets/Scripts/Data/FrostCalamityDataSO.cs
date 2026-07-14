using UnityEngine;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "FrostCalamityData", menuName = "SweetSweeps/Calamities/Frost")]
    public class FrostCalamityDataSO : CalamityDataSO
    {
        [Header("Movement Modifiers")]
        [SerializeField] private float accelerationMultiplier = 0.2f;
        [SerializeField] private float decelerationMultiplier = 0.1f;
        [SerializeField] private float directionChangeMultiplier = 3.5f;

        [Header("Spike Hazards")]
        [SerializeField] private TileHazardOverlay iceSpikeOverlayPrefab;
        [SerializeField] private int spikeCount = 6;
        [SerializeField] private float minSpacing = 3f;

        public override float AccelerationMultiplier => accelerationMultiplier;
        public override float DecelerationMultiplier => decelerationMultiplier;
        public override float DirectionChangeMultiplier => directionChangeMultiplier;

        public TileHazardOverlay IceSpikeOverlayPrefab  => iceSpikeOverlayPrefab;
        public int SpikeCount => spikeCount;
        public float MinSpacing => minSpacing;
    }
}