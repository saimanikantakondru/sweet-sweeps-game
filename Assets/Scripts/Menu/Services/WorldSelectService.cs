using System;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Infrastructure;
using SweetSweeps.Menu.Contracts;

namespace SweetSweeps.Menu.Services
{
    public class WorldSelectService : IWorldSelectService
    {
        private const string StorageKey = "last_world_id";

        private readonly WorldDataSO[] _worlds;

        public WorldDataSO CurrentWorld => _worlds[CurrentIndex];
        public int CurrentIndex { get; private set; }
        public int TotalWorlds => _worlds.Length;

        public event Action<WorldDataSO> OnWorldChanged;

        public WorldSelectService(WorldDataSO[] worlds)
        {
            _worlds = worlds;
            CurrentIndex = ResolveStartIndex();
            Debug.Log($"[WorldSelectService] Initialized. Total worlds={_worlds.Length} Start={CurrentIndex}");
        }

        public void SelectRandom()
        {
            CurrentIndex = PickRandomIndex(_worlds.Length, CurrentIndex);
            Persist();
            Debug.Log($"[WorldSelectService] Selected world={CurrentWorld.WorldId}");
            OnWorldChanged?.Invoke(CurrentWorld);
        }

        private int ResolveStartIndex()
        {
            if (_worlds.Length == 0) return 0;

            string lastId = EncryptedStorage.Get(StorageKey);
            if (string.IsNullOrEmpty(lastId)) return UnityEngine.Random.Range(0, _worlds.Length);

            int index = Array.FindIndex(_worlds, w => w != null && w.WorldId == lastId);
            return index < 0 ? UnityEngine.Random.Range(0, _worlds.Length) : index;
        }

        private void Persist()
        {
            EncryptedStorage.Set(StorageKey, CurrentWorld.WorldId);
        }

        private static int PickRandomIndex(int count, int avoidIndex)
        {
            if (count <= 1) return 0;

            int picked = UnityEngine.Random.Range(0, count);
            if (picked != avoidIndex) return picked;

            return (picked + 1) % count;
        }
    }
}