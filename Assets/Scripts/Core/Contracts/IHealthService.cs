using System;
using SweetSweeps.Core.Services;

namespace SweetSweeps.Core.Contracts
{
    public interface IHealthService
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }

        event Action<int> OnHealthChanged;
        event Action<DamageType> OnDamaged;
        event Action<DamageType> OnDeath;

        void SetIFrameService(IFrameService iFrameService);
        void TakeDamage(int amount, DamageType damageType = DamageType.Default);
        void Heal(int amount);
        void ResetHealth();
    }
}