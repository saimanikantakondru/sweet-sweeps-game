using System;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Level
{
    public class LevelService : ILevelService
    {
        private const int UnsetIndex = -1;

        private readonly ILevelCatalog _catalog;
        private readonly ILevelHistoryService _history;
        private readonly Grid _levelRoot;

        private LevelContentRoot _currentContent;
        private int _currentIndex = UnsetIndex;
        private string _currentWorldId;

        public LevelPresetSO CurrentPreset { get; private set; }
        public LevelContentRoot CurrentContent => _currentContent;
        public int CurrentIndex => _currentIndex;
        public string CurrentWorldId => _currentWorldId;

        public Vector2 PlayerSpawnPosition =>
            _currentContent != null ? _currentContent.PlayerSpawn : Vector2.zero;

        public event Action OnLevelReady;

        public LevelService(
            ILevelCatalog catalog,
            ILevelHistoryService history,
            Grid levelRoot)
        {
            _catalog = catalog;
            _history = history;
            _levelRoot = levelRoot;
        }

        public void LoadNew(string worldId, int avoidIndex)
        {
            if (!TryGetPresets(worldId, out var presets)) return;

            int pickedIndex = PickIndex(worldId, presets.Length, avoidIndex);
            _history?.SetLastPlayedIndex(worldId, pickedIndex);

            LoadByIndex(worldId, presets, pickedIndex);
        }

        public void LoadAt(string worldId, int index)
        {
            if (!TryGetPresets(worldId, out var presets)) return;

            int safe = ((index % presets.Length) + presets.Length) % presets.Length;
            LoadByIndex(worldId, presets, safe);
        }

        public void UnloadCurrent()
        {
            if (_currentContent != null)
            {
                UnityEngine.Object.Destroy(_currentContent.gameObject);
                _currentContent = null;
            }

            CurrentPreset = null;
            Debug.Log("[LevelService] Level unloaded.");
        }

        private bool TryGetPresets(string worldId, out LevelPresetSO[] presets)
        {
            if (!_catalog.TryGetPresets(worldId, out presets))
            {
                Debug.LogError($"[LevelService] No presets found for world '{worldId}'.");
                presets = null;
                return false;
            }
            return true;
        }

        private void LoadByIndex(string worldId, LevelPresetSO[] presets, int index)
        {
            _currentWorldId = worldId;
            _currentIndex = index;
            CurrentPreset = presets[index];

            Debug.Log($"[LevelService] Loaded preset '{CurrentPreset.name}' " +
                      $"({index + 1}/{presets.Length}) for world '{worldId}'.");

            SpawnContent();
            OnLevelReady?.Invoke();
        }

        private int PickIndex(string worldId, int count, int avoidIndex)
        {
            if (count <= 1) return 0;

            int forced = SROptions.Current.ResolveForcedPresetIndex();
            if (forced >= 0)
            {
                if (forced < count) return forced;

                Debug.LogWarning($"[LevelService] Forced preset index {forced} out of range " +
                                 $"for world '{worldId}' (count={count}). Ignoring cheat.");
            }

            return PickRandomIndex(count, avoidIndex);
        }

        private static int PickRandomIndex(int count, int avoidIndex)
        {
            if (count <= 1) return 0;

            int picked = UnityEngine.Random.Range(0, count);
            if (picked != avoidIndex) return picked;

            return (picked + 1) % count;
        }

        private void SpawnContent()
        {
            if (CurrentPreset.LevelContentPrefab == null)
            {
                Debug.LogWarning(
                    $"[LevelService] Preset '{CurrentPreset.name}' has no LevelContentPrefab. " +
                    $"Falling back to scene geometry.");
                return;
            }

            var instance = UnityEngine.Object.Instantiate(
                CurrentPreset.LevelContentPrefab,
                _levelRoot.transform);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            _currentContent = instance.GetComponent<LevelContentRoot>();

            if (_currentContent == null)
                Debug.LogError($"[LevelService] Prefab '{instance.name}' missing LevelContentRoot component.");
        }

    }
}