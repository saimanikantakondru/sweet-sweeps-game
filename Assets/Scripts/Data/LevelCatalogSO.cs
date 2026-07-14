using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "SweetSweeps/LevelCatalog")]
    public class LevelCatalogSO : ScriptableObject, ILevelCatalog
    {
        [SerializeField, AssetsOnly] private LevelPresetSO[] presets;

        private Dictionary<string, LevelPresetSO[]> _byWorld;
        private LevelCatalogEntry[] _entries;

        public IReadOnlyList<LevelCatalogEntry> Entries
        {
            get
            {
                EnsureBuilt();
                return _entries;
            }
        }

        public bool TryGetPresets(string worldId, out LevelPresetSO[] presets)
        {
            EnsureBuilt();
            return _byWorld.TryGetValue(worldId, out presets) && presets.Length > 0;
        }

        private void OnEnable() => _byWorld = null;

        private void EnsureBuilt()
        {
            if (_byWorld != null) return;

            _byWorld = presets
                .Where(preset => preset != null)
                .GroupBy(preset => preset.WorldId)
                .ToDictionary(group => group.Key, group => group.ToArray());

            var entries = new List<LevelCatalogEntry>();
            foreach (var pair in _byWorld)
                for (int i = 0; i < pair.Value.Length; i++)
                    entries.Add(new LevelCatalogEntry(pair.Key, i, pair.Value[i].name));

            _entries = entries.ToArray();
        }
    }
}
