using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SweetSweeps.Gameplay.Calamities
{
    public class TileHazardSpawner
    {
        private const float SurfaceProbeInset = 0.8f;
        private const int SurfaceClearanceCells = 1;

        private readonly Grid _levelRoot;
        private readonly MonoBehaviour _coroutineRunner;
        private readonly LayerMask _groundMask;
        private readonly List<TileHazardOverlay> _spawnedOverlays = new();

        public TileHazardSpawner(Grid levelRoot, MonoBehaviour coroutineRunner, LayerMask groundMask)
        {
            _levelRoot = levelRoot;
            _coroutineRunner = coroutineRunner;
            _groundMask = groundMask;
        }
        
        public void SpawnOnRandomTiles(
            int count,
            TileHazardOverlay prefab,
            float minSpacingWorld   = 3f)
        {
            if (prefab == null)
            {
                Debug.LogWarning("[TileHazardSpawner] Prefab is null.");
                return;
            }

            var parent = _levelRoot.transform;
            var tiles = CollectTiles();

            if (tiles.Count == 0)
            {
                Debug.LogWarning("[TileHazardSpawner] No tiles found.");
                return;
            }

            Physics2D.SyncTransforms();
            var selected = SelectSpread(tiles, count, minSpacingWorld);

            foreach (var entry in selected)
            {
                Vector3 worldPos = entry.Map.GetCellCenterWorld(entry.Position);

                var instance = Object.Instantiate(
                    prefab,
                    worldPos,
                    Quaternion.identity,
                    parent);

                instance.transform.localScale = entry.Map.cellSize;
                instance.Appear();
                _spawnedOverlays.Add(instance);
            }

            Debug.Log($"[TileHazardSpawner] Spawned {_spawnedOverlays.Count} overlays.");
        }

        private List<TileEntry> SelectSpread(List<TileEntry> source, int count, float minSpacing)
        {
            var shuffled = new List<TileEntry>(source);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            var result    = new List<TileEntry>(count);
            var resultPos = new List<Vector2>(count);
            float spacingSqr = minSpacing * minSpacing;

            foreach (var entry in shuffled)
            {
                if (result.Count >= count) break;

                Vector3 wp  = entry.Map.GetCellCenterWorld(entry.Position);
                Vector2 wp2 = new Vector2(wp.x, wp.y);
        
                bool tooClose = false;
                foreach (var taken in resultPos)
                {
                    if ((wp2 - taken).sqrMagnitude < spacingSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;
                if (!IsSurfaceClear(entry, wp)) continue;

                result.Add(entry);
                resultPos.Add(wp2);
            }

            if (result.Count < count)
                Debug.LogWarning($"[TileHazardSpawner] Could only place {result.Count}/{count} " +
                                 $"hazards with spacing={minSpacing}. Reduce minSpacingWorld or count.");

            return result;
        }

        public void DespawnAll()
        {
            _coroutineRunner.StartCoroutine(DespawnRoutine());
        }

        private List<TileEntry> CollectTiles()
        {
            var occupied = new HashSet<Vector3Int>();
            var candidates = new List<(TileEntry entry, Vector3Int cell)>();

            foreach (var map in _levelRoot.GetComponentsInChildren<Tilemap>())
            {
                if (!IsGroundLayer(map.gameObject.layer)) continue;

                map.CompressBounds();

                foreach (var pos in map.cellBounds.allPositionsWithin)
                {
                    if (!map.HasTile(pos)) continue;

                    Vector3Int cell = _levelRoot.WorldToCell(map.GetCellCenterWorld(pos));
                    occupied.Add(cell);
                    candidates.Add((new TileEntry(map, pos), cell));
                }
            }

            var tiles = new List<TileEntry>(candidates.Count);
            foreach (var (entry, cell) in candidates)
                if (!occupied.Contains(cell + Vector3Int.up)
                    && occupied.Contains(cell + Vector3Int.left)
                    && occupied.Contains(cell + Vector3Int.right))
                    tiles.Add(entry);

            return tiles;
        }

        private bool IsSurfaceClear(TileEntry entry, Vector3 cellCenter)
        {
            Vector3 cellSize = entry.Map.cellSize;
            Vector2 probeSize = new Vector2(cellSize.x, cellSize.y) * SurfaceProbeInset;

            for (int step = 1; step <= SurfaceClearanceCells; step++)
            {
                Vector2 probeCenter = new Vector2(cellCenter.x, cellCenter.y + cellSize.y * step);
                if (Physics2D.OverlapBox(probeCenter, probeSize, 0f, _groundMask))
                    return false;
            }

            return true;
        }

        private bool IsGroundLayer(int layer) => (_groundMask.value & (1 << layer)) != 0;

        private IEnumerator DespawnRoutine()
        {
            var routines = _spawnedOverlays
                .Where(o => o != null)
                .Select(o => _coroutineRunner.StartCoroutine(o.DisappearRoutine()))
                .ToList();

            foreach (var routine in routines)
                yield return routine;

            foreach (var overlay in _spawnedOverlays)
                if (overlay != null)
                    Object.Destroy(overlay.gameObject);

            _spawnedOverlays.Clear();
            Debug.Log("[TileHazardSpawner] All overlays removed.");
        }
    }

    public struct TileEntry
    {
        public Tilemap Map;
        public Vector3Int Position;

        public TileEntry(Tilemap map, Vector3Int position)
        {
            Map = map;
            Position = position;
        }
    }
}