using System;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Level;

namespace SweetSweeps.Core.Contracts
{
    public interface ILevelService
    {
        LevelPresetSO CurrentPreset { get; }
        LevelContentRoot CurrentContent { get; }
        Vector2 PlayerSpawnPosition { get; }
        int CurrentIndex { get; }
        string CurrentWorldId { get; }
        event Action OnLevelReady;

        void LoadNew(string worldId, int avoidIndex);
        void LoadAt(string worldId, int index);
        void UnloadCurrent();
    }
}