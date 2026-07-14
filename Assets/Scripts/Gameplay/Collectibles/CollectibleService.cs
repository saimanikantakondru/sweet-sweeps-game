using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Gameplay.Collectibles
{
    public class CollectibleService : ICollectibleService
    {
        private readonly IScoreService _scoreService;
        private readonly IPhaseService _phaseService;
        private readonly ICollectionTracker _collectionTracker;
        private readonly IPurpleCoinValueService _valueService;
        private readonly SpawnPoolService _spawnPool;
        private readonly SpawnPointSelector _selector;
        private readonly List<CollectibleView> _spawnedViews = new();
        private readonly Dictionary<CollectibleType, Transform> _categoryRoots = new();
        
        private Transform _playerTransform;
        private Transform _collectiblesRoot;
        
        public event Action<CollectibleDataSO> OnCollected;
        public event Action<CoinRevealInfo> OnPurpleCoinValueRevealed;

        public CollectibleService(
            IScoreService scoreService,
            ICollectionTracker collectionTracker,
            IPhaseService phaseService,
            IPurpleCoinValueService valueService,
            SpawnPoolService spawnPool,
            SpawnPointSelector selector)
        {
            _scoreService = scoreService;
            _collectionTracker = collectionTracker;
            _phaseService = phaseService;
            _valueService = valueService;
            _spawnPool = spawnPool;
            _selector = selector;
        }
        
        public void SetPlayerTransform(Transform player)
        {
            _playerTransform = player;
        }

        public void RegisterCollected(CollectibleDataSO data, Vector3 worldPosition)
        {
            switch (data.Type)
            {
                case CollectibleType.Coin:
                    _scoreService.RegisterGoldCoin();
                    _collectionTracker.TrackCoinCollected();
                    break;

                case CollectibleType.SsCoin:
                    _scoreService.AddSsCoins(data.ScoreValue);
                    _collectionTracker.TrackSsCoinCollected(_phaseService.CurrentPhase);
                    RevealPurpleCoinValue(worldPosition);
                    break;

                case CollectibleType.Trap:
                    _collectionTracker.TrackSpikedCandyHit();
                    break;

                case CollectibleType.SourCandy:
                    _collectionTracker.TrackSourCandyHit();
                    _scoreService.RegisterSourCandy();
                    break;
            }

            OnCollected?.Invoke(data);
        }

        private void RevealPurpleCoinValue(Vector3 worldPosition)
        {
            if (_valueService.TryGetNextValue(_phaseService.CurrentPhase, out float value))
                OnPurpleCoinValueRevealed?.Invoke(new CoinRevealInfo(value, worldPosition));
        }

        public void SpawnStep(StepData step, LevelPresetSO preset)
        {
            if (!_spawnPool.IsReady)
            {
                Debug.LogWarning("[CollectibleService] SpawnPool not ready.");
                return;
            }

            if (_playerTransform == null)
            {
                Debug.LogWarning("[CollectibleService] PlayerTransform not set.");
                return;
            }

            EnsureHierarchy();

            int totalCount = step.goldCoins + step.purpleCoins + step.sourCandies;

            if (totalCount == 0) return;

            var selectedPoints = _selector.SelectForStep(
                new List<SpawnPoint>(_spawnPool.AllPoints),
                totalCount,
                _playerTransform);

            if (selectedPoints.Count == 0)
            {
                Debug.LogWarning($"[CollectibleService] No spawn points for step={step.step}");
                return;
            }

            foreach (var point in selectedPoints)
                _spawnPool.MarkUsed(point);

            int pointIndex = 0;
            pointIndex = SpawnBatchAtPoints(
                preset.CoinData, step.goldCoins, selectedPoints, ref pointIndex);
            pointIndex = SpawnBatchAtPoints(
                preset.SsCoinData, step.purpleCoins, selectedPoints, ref pointIndex);
            SpawnBatchAtPoints(
                preset.SourCandyData, step.sourCandies, selectedPoints, ref pointIndex);

            Debug.Log($"[CollectibleService] Step={step.step} spawned at {selectedPoints.Count} points.");
        }

        private int SpawnBatchAtPoints(
            CollectibleDataSO data,
            int count,
            List<SpawnPoint> points,
            ref int pointIndex)
        {
            if (data == null || count <= 0) return pointIndex;

            if (data.Prefab == null)
            {
                Debug.LogWarning($"[CollectibleService] Prefab null on {data.name}.");
                return pointIndex;
            }

            Transform parent = GetCategoryRoot(data.Type);

            for (int i = 0; i < count; i++)
            {
                if (pointIndex >= points.Count) break;

                SpawnPoint point = points[pointIndex++];
                Vector3 spawnPos = new Vector3(point.Position.x, point.Position.y + data.SpawnOffsetY, 0f);

                var instance = UnityEngine.Object.Instantiate(
                    data.Prefab, spawnPos, Quaternion.identity, parent);

                if (!instance.TryGetComponent<CollectibleView>(out var view))
                {
                    Debug.LogError($"[CollectibleService] Missing CollectibleView on {data.Prefab.name}");
                    UnityEngine.Object.Destroy(instance);
                    continue;
                }

                view.Initialize(data);
                _spawnedViews.Add(view);
            }

            return pointIndex;
        }
        
        public void DespawnAll()
        {
            _selector.Reset();
            
            foreach (var view in _spawnedViews)
            {
                if (view != null)
                    UnityEngine.Object.Destroy(view.gameObject);
            }

            _spawnedViews.Clear();

            if (_collectiblesRoot != null)
                UnityEngine.Object.Destroy(_collectiblesRoot.gameObject);

            _collectiblesRoot = null;
            _categoryRoots.Clear();

            Debug.Log("[CollectibleService] All collectibles despawned.");
        }

        private void EnsureHierarchy()
        {
            if (_collectiblesRoot != null) return;

            _collectiblesRoot = new GameObject("Collectibles").transform;

            foreach (CollectibleType type in Enum.GetValues(typeof(CollectibleType)))
            {
                var categoryGo = new GameObject(GetCategoryName(type));
                categoryGo.transform.SetParent(_collectiblesRoot);
                _categoryRoots[type] = categoryGo.transform;
            }
        }

        private Transform GetCategoryRoot(CollectibleType type)
        {
            if (_categoryRoots.TryGetValue(type, out var root)) return root;
            return _collectiblesRoot;
        }

        private static string GetCategoryName(CollectibleType type)
        {
            return type switch
            {
                CollectibleType.Coin => "GoldCoins",
                CollectibleType.SsCoin => "SsCoins",
                CollectibleType.SourCandy => "SourCandies",
                _ => type.ToString()
            };
        }
    }
}