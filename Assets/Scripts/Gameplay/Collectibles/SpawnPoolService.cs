using System.Collections.Generic;
using UnityEngine;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class SpawnPoolService
    {
        private List<SpawnPoint> _points = new();

        public IReadOnlyList<SpawnPoint> AllPoints => _points;
        public bool IsReady => _points.Count > 0;

        public void Initialize(List<SpawnPoint> points)
        {
            _points = points;
            Debug.Log($"[SpawnPoolService] Initialized with {_points.Count} points.");
        }

        public void MarkUsed(SpawnPoint point)
        {
            point.MarkUsed();
        }

        public void Reset()
        {
            foreach (var point in _points)
                point.ResetForRound();

            Debug.Log($"[SpawnPoolService] All {_points.Count} points unlocked.");
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform playerTransform)
        {
            if (playerTransform == null) return;

            float playerX = playerTransform != null ? playerTransform.position.x : 0f;

            foreach (var point in _points)
            {
                bool isAhead = point.Position.x > playerX;

                if (!point.IsAvailable)
                    Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                else if (isAhead)
                    Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                else
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

                Gizmos.DrawWireSphere(point.Position, 0.25f);
            }
        }
#endif
    }
}