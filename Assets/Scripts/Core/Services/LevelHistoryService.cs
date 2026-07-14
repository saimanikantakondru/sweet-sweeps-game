using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Infrastructure;

namespace SweetSweeps.Core.Services
{
    public class LevelHistoryService : ILevelHistoryService
    {
        private const string StorageKey = "level_history";
        private const int Unset = -1;

        private Dictionary<string, int> _cache;

        public int GetLastPlayedIndex(string worldId)
        {
            EnsureLoaded();
            return _cache.TryGetValue(worldId, out var index) ? index : Unset;
        }

        public void SetLastPlayedIndex(string worldId, int index)
        {
            if (string.IsNullOrEmpty(worldId)) return;

            EnsureLoaded();
            _cache[worldId] = index;
            Persist();
        }

        public void Clear()
        {
            _cache = new Dictionary<string, int>();
            EncryptedStorage.Remove(StorageKey);
        }

        private void EnsureLoaded()
        {
            if (_cache != null) return;

            _cache = new Dictionary<string, int>();
            var json = EncryptedStorage.Get(StorageKey);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var parsed = JsonUtility.FromJson<LevelHistoryPayload>(json);
                if (parsed?.entries == null) return;

                foreach (var entry in parsed.entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.worldId)) continue;
                    _cache[entry.worldId] = entry.lastPlayedIndex;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelHistoryService] Parse error: {e.Message}");
                EncryptedStorage.Remove(StorageKey);
            }
        }

        private void Persist()
        {
            var payload = new LevelHistoryPayload { entries = new LevelHistoryEntry[_cache.Count] };
            int i = 0;
            foreach (var kv in _cache)
            {
                payload.entries[i++] = new LevelHistoryEntry
                {
                    worldId = kv.Key,
                    lastPlayedIndex = kv.Value,
                };
            }

            EncryptedStorage.Set(StorageKey, JsonUtility.ToJson(payload));
        }

        [Serializable]
        private class LevelHistoryPayload
        {
            public LevelHistoryEntry[] entries;
        }

        [Serializable]
        private class LevelHistoryEntry
        {
            public string worldId;
            public int lastPlayedIndex;
        }
    }
}
