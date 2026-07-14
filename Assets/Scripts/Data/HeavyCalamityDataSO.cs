using UnityEngine;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "HeavyCalamityData", menuName = "SweetSweeps/Calamities/Heavy")]
    public class HeavyCalamityDataSO : CalamityDataSO
    {
        [Header("Movement Modifiers")]
        [SerializeField] private float speedMultiplier = 0.5f;
        [SerializeField] private float accelerationMultiplier = 0.6f;
        [SerializeField] private float decelerationMultiplier = 0.7f;
        [SerializeField] private float jumpForceMultiplier = 0.6f;

        [Header("Lava Tiles")]
        [SerializeField] private TileHazardOverlay lavaOverlayPrefab;
        [SerializeField] private int lavaTileCount = 6;
        [SerializeField] private float minSpacing = 3f;

        public override float SpeedMultiplier => speedMultiplier;
        public override float AccelerationMultiplier => accelerationMultiplier;
        public override float DecelerationMultiplier => decelerationMultiplier;
        public override float JumpForceMultiplier => jumpForceMultiplier;

        public TileHazardOverlay LavaOverlayPrefab  => lavaOverlayPrefab;
        public int LavaTileCount => lavaTileCount;
        public float MinSpacing => minSpacing;
    }
}