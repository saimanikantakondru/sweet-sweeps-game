using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "PlatformData", menuName = "SweetSweeps/PlatformData")]
    public class PlatformDataSO : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float pauseDuration = 0.8f;

        public float MoveSpeed => moveSpeed;
        public float PauseDuration => pauseDuration;
    }
}