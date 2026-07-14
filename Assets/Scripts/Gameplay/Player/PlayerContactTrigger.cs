using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Calamities;
using SweetSweeps.Gameplay.Collectibles;

namespace SweetSweeps.Gameplay.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerContactTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask collectibleLayer;
        [SerializeField] private LayerMask hazardLayer;
        
        private IHealthService _healthService;
        private ICollectibleService _collectibleService;
        private IKnockbackService _knockbackService;
        private bool _initialized;

        public void Initialize(IHealthService healthService, ICollectibleService collectibleService, IKnockbackService knockbackService)
        {
            _healthService = healthService;
            _collectibleService = collectibleService;
            _knockbackService = knockbackService;
            _initialized = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_initialized) return;

            int layer = 1 << other.gameObject.layer;

            if ((layer & collectibleLayer) != 0)
                HandleCollectible(other);
            else if ((layer & hazardLayer) != 0)
                HandleHazardEnter(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_initialized) return;

            int layer = 1 << other.gameObject.layer;

            if ((layer & hazardLayer) != 0)
                HandleHazardStay(other);
        }

        private void HandleCollectible(Collider2D other)
        {
            if (!other.TryGetComponent<CollectibleView>(out var view)) return;

            var data = view.Data;

            if (data.Type == CollectibleType.Trap)
            {
                _healthService.TakeDamage(data.DamageValue, DamageType.Collectibles);
                _knockbackService.Apply(other.transform.position);
                //Debug.Log($"[PlayerContactTrigger] Trap hit: {data.name} damage={data.DamageValue}");
            }

            _collectibleService.RegisterCollected(data, other.transform.position);
            view.ForceCollect();
        }

        private void HandleHazardEnter(Collider2D other)
        {
            if (!other.TryGetComponent<CalamityObstacleView>(out var obstacle)) return;
            _healthService.TakeDamage(obstacle.DamageAmount, DamageType.BiomeCalamity);
            _knockbackService.Apply(other.transform.position);
            Debug.Log($"[PlayerContactTrigger] Hazard enter: {other.gameObject.name}");
        }

        private void HandleHazardStay(Collider2D other)
        {
            if (!other.TryGetComponent<CalamityObstacleView>(out var obstacle)) return;
            _knockbackService.Apply(other.transform.position);
            _healthService.TakeDamage(obstacle.DamageAmount, DamageType.BiomeCalamity);
        }
    }
}