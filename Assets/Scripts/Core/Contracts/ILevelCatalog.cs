using System.Collections.Generic;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Contracts
{
    public readonly struct LevelCatalogEntry
    {
        public readonly string WorldId;
        public readonly int LocalIndex;
        public readonly string Name;

        public LevelCatalogEntry(string worldId, int localIndex, string name)
        {
            WorldId = worldId;
            LocalIndex = localIndex;
            Name = name;
        }
    }

    public interface ILevelCatalog
    {
        IReadOnlyList<LevelCatalogEntry> Entries { get; }
        bool TryGetPresets(string worldId, out LevelPresetSO[] presets);
    }
}
