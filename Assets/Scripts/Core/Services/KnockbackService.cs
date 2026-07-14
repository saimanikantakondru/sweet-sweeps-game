using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;

namespace SweetSweeps.Core.Services
{
    public class KnockbackService : IKnockbackService
    {
        readonly Rigidbody2D  _rigidbody;
        readonly PlayerStatsSO _stats;

        public KnockbackService(Rigidbody2D rigidbody, PlayerStatsSO stats)
        {
            _rigidbody = rigidbody;
            _stats     = stats;
        }

        public void Apply(Vector2 sourcePosition)
        {
            if (!_stats.AllowKnockback) return;
            
            Vector2 playerPos = _rigidbody.position;
            float horizontal = playerPos.x - sourcePosition.x;
            Vector2 direction = new Vector2(Mathf.Sign(horizontal), 1f).normalized;
            
            _rigidbody.linearVelocity = new Vector2(0f, 0f);

            _rigidbody.AddForce(direction * _stats.KnockbackForce, ForceMode2D.Impulse);

            Debug.Log($"[KnockbackService] dir={direction} force={_stats.KnockbackForce}");
        }
    }
}