using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "SpawnSystemSettings",
        menuName = "SweetSweeps/SpawnSystemSettings")]
    public class SpawnSystemSettingsSO : ScriptableObject
    {
        [Header("Scanner")]
        [SerializeField] private float heightAboveGround = 1.5f;
        [SerializeField] private float minDistanceBetweenPoints = 1.0f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Selector")]
        [SerializeField] private float minAheadDistance = 0.5f;

        public float HeightAboveGround => heightAboveGround;
        public float MinDistanceBetweenPoints => minDistanceBetweenPoints;
        public LayerMask GroundMask => groundMask;
        public LayerMask ObstacleMask => obstacleMask;
        public float MinAheadDistance => minAheadDistance;
    }
}