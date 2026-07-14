using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Services
{
    public class HealthService : IHealthService
    {
        private readonly PlayerStatsSO _stats;
        private IFrameService _iFrameService;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => _stats.MaxHealth;
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int> OnHealthChanged;
        public event Action<DamageType> OnDamaged;
        public event Action<DamageType> OnDeath;

        public HealthService(PlayerStatsSO stats)
        {
            _stats = stats;
            CurrentHealth = stats.MaxHealth;
        }

        public void SetIFrameService(IFrameService iFrameService)
        {
            _iFrameService = iFrameService;
        }

        public void TakeDamage(int amount, DamageType damageType = DamageType.Default)
        {
            if (IsDead) return;
            if (_iFrameService != null && _iFrameService.IsInvincible && damageType != DamageType.Fall) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            //Debug.Log($"[HealthService] Damage={amount} health={CurrentHealth}/{MaxHealth}");
            OnHealthChanged?.Invoke(CurrentHealth);
            if (damageType != DamageType.Fall)
            {
                OnDamaged?.Invoke(damageType);
            }

            if (IsDead)
            {
                //Debug.Log("[HealthService] Player died.");
                OnDeath?.Invoke(damageType);
                return;
            }

            _iFrameService?.TriggerInvincibility(false);
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            //Debug.Log($"[HealthService] Heal={amount} health={CurrentHealth}/{MaxHealth}");
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth);
            //Debug.Log("[HealthService] Health reset.");
        }
    }
    
    public enum DamageType
    {
        Default,
        Collectibles,
        BiomeCalamity,
        Fall
    }
}