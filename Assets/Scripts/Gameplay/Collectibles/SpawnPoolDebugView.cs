using UnityEngine;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class SpawnPoolDebugView : MonoBehaviour
    {
        private SpawnPoolService _poolService;
        private Transform _playerTransform;

        public void Initialize(SpawnPoolService poolService, Transform playerTransform)
        {
            _poolService = poolService;
            _playerTransform = playerTransform;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            _poolService?.DrawGizmos(_playerTransform);
#endif
        }
    }
}