using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Player;

namespace SweetSweeps.Gameplay.Calamities
{
    public class HeavyCalamityEffect : ICalamityEffect, IMovementModifierSource
    {
        private readonly HeavyCalamityDataSO _data;
        private readonly IMovementModifierRegistry _modifierRegistry;
        private readonly TileHazardSpawner _tileSpawner;

        private bool _isActive;
        private bool _hazardsSpawned;

        public HeavyCalamityEffect(
            HeavyCalamityDataSO data,
            IMovementModifierRegistry modifierRegistry,
            Grid levelGrid,
            MonoBehaviour coroutineRunner,
            LayerMask groundMask)
        {
            _data = data;
            _modifierRegistry = modifierRegistry;
            _tileSpawner = new TileHazardSpawner(levelGrid, coroutineRunner, groundMask);
        }

        public MovementModifier GetModifier()
        {
            if (!_isActive) return MovementModifier.Identity;

            return new MovementModifier
            {
                SpeedMultiplier = _data.SpeedMultiplier,
                AccelerationMultiplier = _data.AccelerationMultiplier,
                DecelerationMultiplier = _data.DecelerationMultiplier,
                DirectionChangeMultiplier = _data.DirectionChangeMultiplier,
                JumpForceMultiplier = _data.JumpForceMultiplier
            };
        }

        public void Apply(CalamityLevel level, float intensity)
        {
            if (level == CalamityLevel.None)
            {
                Remove();
                return;
            }

            if (!_isActive)
            {
                _isActive = true;
                _modifierRegistry.Register(this);
                Debug.Log("[HeavyCalamityEffect] LevelOne — movement modifiers applied.");
            }

            if (level == CalamityLevel.LevelTwo && !_hazardsSpawned)
            {
                _hazardsSpawned = true;
                _tileSpawner.SpawnOnRandomTiles(_data.LavaTileCount, _data.LavaOverlayPrefab, _data.MinSpacing);
                Debug.Log("[HeavyCalamityEffect] LevelTwo — lava tiles spawned.");
            }
        }

        public void Remove()
        {
            if (!_isActive) return;

            _isActive = false;
            _hazardsSpawned = false;
            _modifierRegistry.Unregister(this);
            _tileSpawner.DespawnAll();
            Debug.Log("[HeavyCalamityEffect] Removed.");
        }
    }
}