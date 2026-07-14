using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class SpawnPointScanner
    {
        private readonly float _heightAboveGround;
        private readonly float _minDistanceBetweenPoints;
        private readonly LayerMask _groundMask;
        private readonly LayerMask _obstacleMask;

        public SpawnPointScanner(
            float heightAboveGround,
            float minDistanceBetweenPoints,
            LayerMask groundMask,
            LayerMask obstacleMask)
        {
            _heightAboveGround = heightAboveGround;
            _minDistanceBetweenPoints = minDistanceBetweenPoints;
            _groundMask = groundMask;
            _obstacleMask = obstacleMask;
        }

        public List<SpawnPoint> ScanFromTilemap(Grid levelGrid)
        {
            var candidates = new List<Vector2>();
            var tilemaps = levelGrid.GetComponentsInChildren<Tilemap>();

            foreach (var map in tilemaps)
            {
                map.CompressBounds();

                foreach (var cellPos in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(cellPos)) continue;

                    Vector3 worldPos = map.CellToWorld(cellPos);
                    Vector2 spawnPos = new Vector2(worldPos.x + 0.5f,
                                                   worldPos.y + _heightAboveGround);

                    if (IsValidSpawnPosition(spawnPos))
                        candidates.Add(spawnPos);
                }
            }

            return FilterByMinDistance(candidates);
        }

        private bool IsValidSpawnPosition(Vector2 position)
        {
            var groundHit = Physics2D.Raycast(
                position,
                Vector2.down,
                _heightAboveGround,
                _groundMask);

            if (!groundHit) return false;

            var overlapHit = Physics2D.OverlapCircle(position, 0.3f, _obstacleMask);
            if (overlapHit) return false;

            return true;
        }

        private List<SpawnPoint> FilterByMinDistance(List<Vector2> candidates)
        {
            var result = new List<SpawnPoint>();

            foreach (var candidate in candidates)
            {
                bool tooClose = false;

                foreach (var existing in result)
                {
                    if (Vector2.Distance(candidate, existing.Position) < _minDistanceBetweenPoints)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                    result.Add(new SpawnPoint(candidate));
            }

            Debug.Log($"[SpawnPointScanner] Found {result.Count} valid spawn points.");
            return result;
        }
    }
}