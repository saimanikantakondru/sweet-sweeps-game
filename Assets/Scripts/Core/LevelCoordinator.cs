using System;
using System.Collections;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Collectibles;
using SweetSweeps.Gameplay.Level;
using SweetSweeps.Gameplay.Player;
using SweetSweeps.Infrastructure;

namespace SweetSweeps.Core
{
    public class LevelCoordinator
    {
        private readonly ILevelService _levelService;
        private readonly ILevelHistoryService _levelHistory;
        private readonly ICollectibleService _collectibleService;
        private readonly IPlatformService _platformService;
        private readonly IPhaseService _phaseService;
        private readonly ICameraService _cameraService;
        private readonly SpawnPointScanner _spawnScanner;
        private readonly SpawnPoolService _spawnPool;
        private readonly PlayerController _playerController;
        private readonly RespawnService _respawnService;
        private readonly CoroutineRunner _coroutineRunner;

        private Grid _levelGrid;
        private SpawnPoolDebugView _debugView;
        private string _startWorldId;
        private bool _configured;

        public LevelPresetSO CurrentPreset => _levelService.CurrentPreset;
        public Vector2 PlayerSpawnPosition => _levelService.PlayerSpawnPosition;
        public int CurrentLevelIndex => _levelService.CurrentIndex;
        public string CurrentWorldId => _levelService.CurrentWorldId;

        public LevelCoordinator(
            ILevelService levelService,
            ILevelHistoryService levelHistory,
            ICollectibleService collectibleService,
            IPlatformService platformService,
            IPhaseService phaseService,
            ICameraService cameraService,
            SpawnPointScanner spawnScanner,
            SpawnPoolService spawnPool,
            PlayerController playerController,
            RespawnService respawnService,
            CoroutineRunner coroutineRunner)
        {
            _levelService = levelService;
            _levelHistory = levelHistory;
            _collectibleService = collectibleService;
            _platformService = platformService;
            _phaseService = phaseService;
            _cameraService = cameraService;
            _spawnScanner = spawnScanner;
            _spawnPool = spawnPool;
            _playerController = playerController;
            _respawnService = respawnService;
            _coroutineRunner = coroutineRunner;
        }

        public void Configure(Grid levelGrid, SpawnPoolDebugView debugView, string startWorldId)
        {
            _levelGrid = levelGrid;
            _debugView = debugView;
            _startWorldId = startWorldId;
            _configured = true;
        }

        public void LoadNewSession(Action onReady)
        {
            EnsureConfigured();

            int avoid = _levelHistory.GetLastPlayedIndex(_startWorldId);
            LoadInternal(onReady, () => _levelService.LoadNew(_startWorldId, avoid));
        }

        public void LoadResumed(int savedIndex, Action onReady)
        {
            EnsureConfigured();

            LoadInternal(onReady, () => _levelService.LoadAt(_startWorldId, savedIndex));
        }

        public void Reload(Action onReady)
        {
            EnsureConfigured();

            int avoidIndex = _levelService.CurrentIndex;

            _cameraService.ClearConfinerBounds();
            _spawnPool.Reset();
            _collectibleService.DespawnAll();
            _levelService.UnloadCurrent();

            LoadInternal(onReady, () => _levelService.LoadNew(_startWorldId, avoidIndex));
        }

        public void PlacePlayerAt(Vector3 position, bool safeRespawn = false)
        {
            if (safeRespawn)
                _respawnService.RespawnAt(_playerController.transform, position, PlayerSpawnPosition);
            else
                _playerController.transform.position = position;
        }

        private void LoadInternal(Action onReady, Action loadAction)
        {
            void Handler()
            {
                _levelService.OnLevelReady -= Handler;
                SetupLevel();
                onReady?.Invoke();
            }

            _levelService.OnLevelReady += Handler;
            loadAction();
        }

        private void SetupLevel()
        {
            var preset = _levelService.CurrentPreset;
            _phaseService.SetPresetBinding(preset.CalamityBinding);

            var points = _spawnScanner.ScanFromTilemap(_levelGrid);
            _spawnPool.Initialize(points);

            if (_collectibleService is CollectibleService cs)
                cs.SetPlayerTransform(_playerController.transform);

            _debugView?.Initialize(_spawnPool, _playerController.transform);

            ApplyConfinerBounds();

            RegisterPlatforms();

            _coroutineRunner.StartCoroutine(RecalibrateParallaxAfterCameraCut());

            Debug.Log($"[LevelCoordinator] Level ready. SpawnPoints={points.Count}");
        }

        private void RegisterPlatforms()
        {
            _platformService.Clear();

            var content = _levelService.CurrentContent;
            if (content == null) return;

            var platforms = content.PlatformMovers;
            for (int i = 0; i < platforms.Count; i++)
                _platformService.Register(platforms[i]);
        }

        private IEnumerator RecalibrateParallaxAfterCameraCut()
        {
            yield return null;

            var cam = Camera.main;
            if (cam == null) yield break;

            _cameraService.SnapToTarget();

            Vector3 settlePos = cam.transform.position;
            int cutGuard = 10;
            while (cutGuard-- > 0)
            {
                yield return null;
                Vector3 p = cam.transform.position;
                if (Vector3.Distance(p, settlePos) <= 0.0005f) break;
                settlePos = p;
            }

            var content = _levelService.CurrentContent;
            if (content == null) yield break;

            Vector2 cameraTravel = ResolveParallaxTravel(content, cam);
            foreach (var m in content.GetComponentsInChildren<ParallaxManager>(true))
                m.RecalibrateBaseline(cameraTravel);
        }

        private Vector2 ResolveParallaxTravel(LevelContentRoot content, Camera cam)
        {
            Vector2 camNow = (Vector2)cam.transform.position;

            var confiner = content.ConfinerBounds;
            if (confiner == null || !cam.orthographic) return Vector2.zero;

            Vector2 framingOffset = camNow - (Vector2)_playerController.transform.position;
            Vector2 desiredSpawnCam = PlayerSpawnPosition + framingOffset;

            Bounds b = confiner.bounds;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float refX = b.size.x > 2f * halfW ? Mathf.Clamp(desiredSpawnCam.x, b.min.x + halfW, b.max.x - halfW) : b.center.x;
            float refY = b.size.y > 2f * halfH ? Mathf.Clamp(desiredSpawnCam.y, b.min.y + halfH, b.max.y - halfH) : b.center.y;

            return camNow - new Vector2(refX, refY);
        }

        private void ApplyConfinerBounds()
        {
            var content = _levelService.CurrentContent;
            if (content == null)
            {
                Debug.LogWarning("[LevelCoordinator] No LevelContentRoot — confiner bounds not applied.");
                return;
            }

            var bounds = content.ConfinerBounds;

            if (bounds == null)
            {
                bounds = GenerateRuntimeBoundsCollider(content);
                if (bounds == null)
                {
                    Debug.LogWarning("[LevelCoordinator] Could not determine confiner bounds for this level.");
                    return;
                }

                content.AssignConfinerBounds(bounds);
            }

            _cameraService.SetConfinerBounds(bounds);
        }

        private static Collider2D GenerateRuntimeBoundsCollider(LevelContentRoot content)
        {
            var boundsValue = LevelContentRoot.ComputeTilemapBounds(content, content.ConfinerMargin);
            if (!boundsValue.HasValue) return null;

            var b = boundsValue.Value;

            var go = new GameObject("Auto_ConfinerBounds");
            go.transform.SetParent(content.transform, true);
            go.transform.position = b.center;

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.offset = Vector2.zero;
            box.size = b.size;

            Debug.Log($"[LevelCoordinator] Auto-generated confiner bounds. Size={b.size}.");
            return box;
        }

        private void EnsureConfigured()
        {
            if (_configured) return;
            throw new InvalidOperationException(
                "[LevelCoordinator] Configure(...) must be called before loading a level.");
        }
    }
}
