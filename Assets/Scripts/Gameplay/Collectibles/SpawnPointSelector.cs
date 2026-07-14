using System.Collections.Generic;
using UnityEngine;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class SpawnPointSelector
    {
        private readonly Camera _camera;
        private readonly float _minAheadDistance;
        private readonly List<SpawnPoint> _candidatePoints = new();
        private readonly List<SpawnPoint> _visiblePoints = new();

        private bool _reversed;

        public SpawnPointSelector(Camera camera, float minAheadDistance = 0.5f)
        {
            _camera = camera;
            _minAheadDistance = minAheadDistance;
        }
        
        public void Reset()
        {
            _reversed = false;
        }
        
        public List<SpawnPoint> SelectForStep(
            List<SpawnPoint> allPoints,
            int countNeeded,
            Transform playerTransform)
        {
            if (allPoints == null || allPoints.Count == 0
                || countNeeded <= 0 || playerTransform == null)
                return new List<SpawnPoint>();

            float playerX = playerTransform.position.x;
            var bounds = GetCameraBounds();

            if (!_reversed)
            {
                _candidatePoints.Clear();
                for (int i = 0; i < allPoints.Count; i++)
                {
                    var p = allPoints[i];
                    if (p.IsAvailable && p.Position.x > playerX + _minAheadDistance)
                        _candidatePoints.Add(p);
                }

                Debug.Log($"aheadAvailable.Count {_candidatePoints.Count}");

                if (_candidatePoints.Count >= countNeeded)
                {
                    _visiblePoints.Clear();
                    for (int i = 0; i < _candidatePoints.Count; i++)
                    {
                        if (bounds.Contains(_candidatePoints[i].Position))
                            _visiblePoints.Add(_candidatePoints[i]);
                    }

                    if (_visiblePoints.Count >= countNeeded)
                    {
                        return SelectRandom(_visiblePoints, countNeeded);
                    }

                    _candidatePoints.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));
                    return SelectMixed(_candidatePoints, countNeeded, bounds);
                }

                _reversed = true;
                Debug.Log("[SpawnPointSelector] No points ahead — switching to reversed phase.");
            }
            
            _candidatePoints.Clear();
            for (int i = 0; i < allPoints.Count; i++)
            {
                var p = allPoints[i];
                if (p.Position.x < playerX - _minAheadDistance)
                {
                    p.ForceUnlock();
                    if (p.IsAvailable)
                        _candidatePoints.Add(p);
                }
            }
            _candidatePoints.Sort((a, b) => b.Position.x.CompareTo(a.Position.x));

            if (_candidatePoints.Count == 0)
            {
                Debug.LogWarning("[SpawnPointSelector] No spawn points found.");
                return new List<SpawnPoint>();
            }

            _visiblePoints.Clear();
            for (int i = 0; i < _candidatePoints.Count; i++)
            {
                if (bounds.Contains(_candidatePoints[i].Position))
                    _visiblePoints.Add(_candidatePoints[i]);
            }

            if (_visiblePoints.Count >= countNeeded)
                return SelectRandom(_visiblePoints, countNeeded);

            return SelectMixed(_candidatePoints, countNeeded, bounds);
        }
        
        private static List<SpawnPoint> SelectMixed(List<SpawnPoint> sortedCandidates, int count, Bounds cameraBounds)
        {
            var inView = new List<SpawnPoint>(count);
            var outView = new List<SpawnPoint>(count);

            for (int i = 0; i < sortedCandidates.Count; i++)
            {
                var p = sortedCandidates[i];
                if (cameraBounds.Contains(p.Position))
                    inView.Add(p);
                else
                    outView.Add(p);
            }

            for (int i = inView.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (inView[i], inView[j]) = (inView[j], inView[i]);
            }

            var result = new List<SpawnPoint>(count);
            result.AddRange(inView);

            for (int i = 0; i < outView.Count && result.Count < count; i++)
                result.Add(outView[i]);

            return result;
        }

        private static List<SpawnPoint> SelectRandom(List<SpawnPoint> candidates, int count)
        {
            if (candidates.Count <= count)
                return new List<SpawnPoint>(candidates);
            
            var pool = new List<SpawnPoint>(candidates);
            var result = new List<SpawnPoint>(count);

            for (int i = 0; i < count; i++)
            {
                int j = Random.Range(i, pool.Count);
                (pool[i], pool[j]) = (pool[j], pool[i]);
                result.Add(pool[i]);
            }

            return result;
        }

        private Bounds GetCameraBounds()
        {
            float height = _camera.orthographicSize * 2f;
            float width = height * _camera.aspect;
            Vector3 pos = _camera.transform.position;

            return new Bounds(
                new Vector3(pos.x, pos.y, 0f),
                new Vector3(width, height, 100f));
        }
    }
}