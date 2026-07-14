using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SweetSweeps.Gameplay.Level
{
    public class LevelContentRoot : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Transform playerSpawn;

        [Header("Tilemaps")]
        [SerializeField, Tooltip("Optional explicit list. If empty, all child Tilemaps are used.")]
        private Tilemap[] tilemapLayers;

        [Header("Camera Confiner")]
        [SerializeField, Tooltip("Collider2D used as Cinemachine BoundingShape2D. If null, LevelCoordinator falls back to auto-rect from tilemaps.")]
        private Collider2D confinerBounds;
        [SerializeField, Tooltip("Margin (world units) added to auto-generated confiner rect.")]
        private float confinerMargin = 1.5f;

        [Header("Gizmo Display")]
        [SerializeField] private bool drawGizmos = true;

        private Tilemap[] _cachedTilemaps;
        private PlatformMover[] _cachedPlatforms;

        public Vector2 PlayerSpawn =>
            playerSpawn != null ? (Vector2)playerSpawn.position : Vector2.zero;

        public Collider2D ConfinerBounds => confinerBounds;
        public float ConfinerMargin => confinerMargin;

        public IReadOnlyList<Tilemap> TilemapLayers
        {
            get
            {
                if (tilemapLayers != null && tilemapLayers.Length > 0)
                    return tilemapLayers;

                if (_cachedTilemaps == null || _cachedTilemaps.Length == 0)
                    _cachedTilemaps = GetComponentsInChildren<Tilemap>(true);

                return _cachedTilemaps;
            }
        }

        public IReadOnlyList<PlatformMover> PlatformMovers
        {
            get
            {
                if (_cachedPlatforms == null || _cachedPlatforms.Length == 0)
                    _cachedPlatforms = GetComponentsInChildren<PlatformMover>(true);
                return _cachedPlatforms;
            }
        }

        public void AssignConfinerBounds(Collider2D bounds) => confinerBounds = bounds;

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            DrawPlayerSpawn();
            DrawConfinerBounds();
        }

        private void DrawPlayerSpawn()
        {
            if (playerSpawn == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerSpawn.position, 0.4f);
            Gizmos.DrawWireCube(
                playerSpawn.position + Vector3.up * 0.8f,
                new Vector3(0.4f, 0.8f, 0f));

#if UNITY_EDITOR
            Handles.Label(playerSpawn.position + Vector3.up * 1.6f, "SPAWN");
#endif
        }

        private void DrawConfinerBounds()
        {
            if (confinerBounds == null) return;

            Gizmos.color = new Color(0.2f, 0.9f, 0.9f, 0.5f);
            var b = confinerBounds.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }

#if UNITY_EDITOR
        [Button("Cache Tilemaps From Children")]
        private void CacheTilemaps()
        {
            tilemapLayers = GetComponentsInChildren<Tilemap>(true);
            EditorUtility.SetDirty(this);
        }

        [Button("Auto-Generate Confiner From Tilemap Bounds")]
        private void GenerateConfinerFromTilemaps()
        {
            var bounds = ComputeTilemapBounds(this, confinerMargin);
            if (!bounds.HasValue)
            {
                Debug.LogWarning("[LevelContentRoot] No tilemap renderers found to compute bounds.");
                return;
            }

            const string shapeName = "ConfinerShape";
            var existing = transform.Find(shapeName);
            if (existing != null) DestroyImmediate(existing.gameObject);

            var shapeGo = new GameObject(shapeName);
            Undo.RegisterCreatedObjectUndo(shapeGo, "Generate Confiner");
            shapeGo.transform.SetParent(transform, true);
            shapeGo.transform.position = bounds.Value.center;

            var poly = shapeGo.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;

            Vector2 half = bounds.Value.extents;
            poly.points = new[]
            {
                new Vector2(-half.x, -half.y),
                new Vector2( half.x, -half.y),
                new Vector2( half.x,  half.y),
                new Vector2(-half.x,  half.y),
            };

            confinerBounds = poly;
            EditorUtility.SetDirty(this);
            Debug.Log($"[LevelContentRoot] Confiner generated. Size={bounds.Value.size}.");
        }
#endif

        public static Bounds? ComputeTilemapBounds(LevelContentRoot content, float margin)
        {
            if (content == null || content.TilemapLayers == null) return null;

            bool initialized = false;
            Bounds union = default;

            foreach (var map in content.TilemapLayers)
            {
                if (map == null) continue;

                map.CompressBounds();

                var renderer = map.GetComponent<TilemapRenderer>();
                if (renderer == null) continue;

                var b = renderer.bounds;
                if (b.size == Vector3.zero) continue;

                if (!initialized)
                {
                    union = b;
                    initialized = true;
                }
                else
                {
                    union.Encapsulate(b);
                }
            }

            if (!initialized) return null;

            union.Expand(margin * 2f);
            return union;
        }
    }
}
