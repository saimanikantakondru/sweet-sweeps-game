using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Level;
using SweetSweeps.Gameplay.Player;

namespace SweetSweeps.Gameplay.Calamities
{
    public class WorldDecayCalamityEffect : ICalamityEffect, IMovementModifierSource
    {
        private readonly WorldDecayCalamityDataSO  _data;
        private readonly IMovementModifierRegistry _modifierRegistry;
        private readonly Grid                      _levelGrid;
        private readonly MonoBehaviour             _coroutineRunner;
        private readonly Transform                 _playerTransform;

        private bool      _isActive;
        private Coroutine _decayCoroutine;

        private readonly List<TileHazardOverlay>                            _hiddenOverlays  = new();
        private readonly List<GameObject>                                   _hiddenDecor    = new();
        private readonly List<(Tilemap map, Vector3Int pos, TileBase tile)> _removedTiles   = new();

        readonly struct DecayEntry
        {
            public readonly float              WorldX;
            public readonly float              WorldY;
            public readonly TileEntry?         Tile;
            public readonly TileHazardOverlay  Overlay;
            public readonly GameObject         Decor;

            public DecayEntry(TileEntry tile, float worldX, float worldY)
            {
                WorldX  = worldX;
                WorldY  = worldY;
                Tile    = tile;
                Overlay = null;
                Decor   = null;
            }

            public DecayEntry(TileHazardOverlay overlay)
            {
                Vector3 p = overlay.transform.position;
                WorldX  = p.x;
                WorldY  = p.y;
                Tile    = null;
                Overlay = overlay;
                Decor   = null;
            }

            public DecayEntry(GameObject decor)
            {
                Vector3 p = decor.transform.position;
                WorldX  = p.x;
                WorldY  = p.y;
                Tile    = null;
                Overlay = null;
                Decor   = decor;
            }
        }

        public WorldDecayCalamityEffect(
            WorldDecayCalamityDataSO  data,
            IMovementModifierRegistry modifierRegistry,
            Grid                      levelGrid,
            MonoBehaviour             coroutineRunner,
            Transform                 playerTransform)
        {
            _data             = data;
            _modifierRegistry = modifierRegistry;
            _levelGrid        = levelGrid;
            _coroutineRunner  = coroutineRunner;
            _playerTransform  = playerTransform;
        }

        public MovementModifier GetModifier() => MovementModifier.Identity;

        public void Apply(CalamityLevel level, float intensity)
        {
            _isActive       = true;
            _decayCoroutine = _coroutineRunner.StartCoroutine(DecayRoutine());
            _modifierRegistry.Register(this);
            Debug.Log("[WorldDecayCalamityEffect] Applied.");
        }

        public void Remove()
        {
            _isActive = false;
            _modifierRegistry.Unregister(this);

            if (_decayCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_decayCoroutine);
                _decayCoroutine = null;
            }

            RestoreTiles();
            RestoreOverlays();
            RestoreDecor();
            Debug.Log("[WorldDecayCalamityEffect] Removed.");
        }
        
        private IEnumerator DecayRoutine()
        {
            float startX = _playerTransform.position.x - _data.DecayStartOffset;
            List<DecayEntry> sorted = BuildSortedDecayList(startX);

            Debug.Log($"[WorldDecayCalamityEffect] Decay starts X={startX:F1} " +
                      $"entries={sorted.Count} " +
                      $"(tiles={sorted.Count(e => e.Tile.HasValue)} " +
                      $"overlays={sorted.Count(e => e.Overlay != null)})");

            var tileWait = new WaitForSeconds(_data.TileRemoveInterval);
            var waveWait = new WaitForSeconds(_data.WaveInterval);

            int index = 0;
            while (_isActive && index < sorted.Count)
            {
                for (int i = 0; i < _data.TilesPerWave && index < sorted.Count; i++, index++)
                {
                    ProcessEntry(sorted[index]);
                    yield return tileWait;
                }

                yield return waveWait;
            }

            Debug.Log("[WorldDecayCalamityEffect] Decay complete.");
        }

        private void ProcessEntry(in DecayEntry entry)
        {
            if (entry.Tile.HasValue)
            {
                var t    = entry.Tile.Value;
                var tile = t.Map.GetTile(t.Position);
                _removedTiles.Add((t.Map, t.Position, tile));
                t.Map.SetTile(t.Position, null);
            }
            else if (entry.Overlay != null)
            {
                entry.Overlay.gameObject.SetActive(false);
                _hiddenOverlays.Add(entry.Overlay);
            }
            else if (entry.Decor != null)
            {
                entry.Decor.SetActive(false);
                _hiddenDecor.Add(entry.Decor);
            }
        }
        
        private List<DecayEntry> BuildSortedDecayList(float startX)
        {
            var result = new List<DecayEntry>();

            foreach (var t in CollectTiles())
            {
                float wx = t.Map.CellToWorld(t.Position).x;
                float wy = t.Map.CellToWorld(t.Position).y;
                if (wx >= startX)
                    result.Add(new DecayEntry(t, wx, wy));
            }

            foreach (var overlay in _levelGrid.GetComponentsInChildren<TileHazardOverlay>())
            {
                if (overlay.gameObject.activeSelf && overlay.transform.position.x >= startX)
                    result.Add(new DecayEntry(overlay));
            }

            foreach (var decor in _levelGrid.GetComponentsInChildren<DecorPiece>())
            {
                if (decor.gameObject.activeSelf && decor.transform.position.x >= startX)
                    result.Add(new DecayEntry(decor.gameObject));
            }

            result.Sort((a, b) =>
            {
                int xCmp = a.WorldX.CompareTo(b.WorldX);
                return xCmp != 0 ? xCmp : a.WorldY.CompareTo(b.WorldY);
            });

            return result;
        }
        
        private List<TileEntry> CollectTiles()
        {
            var tiles = new List<TileEntry>();
            foreach (var map in _levelGrid.GetComponentsInChildren<Tilemap>())
            {
                map.CompressBounds();
                foreach (var pos in map.cellBounds.allPositionsWithin)
                    if (map.GetTile(pos) != null)
                        tiles.Add(new TileEntry(map, pos));
            }

            return tiles;
        }
        
        private void RestoreTiles()
        {
            foreach (var (map, pos, tile) in _removedTiles)
                map.SetTile(pos, tile);

            _removedTiles.Clear();
        }

        private void RestoreOverlays()
        {
            foreach (var overlay in _hiddenOverlays)
                if (overlay != null)
                    overlay.gameObject.SetActive(true);

            _hiddenOverlays.Clear();
        }

        private void RestoreDecor()
        {
            foreach (var decor in _hiddenDecor)
                if (decor != null)
                    decor.SetActive(true);

            _hiddenDecor.Clear();
        }
    }
}