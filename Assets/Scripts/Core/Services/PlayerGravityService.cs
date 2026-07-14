using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class PlayerGravityService : IPlayerGravityService
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly PlayerStatsSO _stats;

        private bool _enabled = true;
        private bool _isFalling;
        private bool _isGrounded;

        public float CurrentGravity { get; private set; }

        public PlayerGravityService(Rigidbody2D rigidbody, PlayerStatsSO stats)
        {
            _rigidbody = rigidbody;
            _stats = stats;
        }

        public void SetFalling(bool isFalling)
        {
            if (_isGrounded) return;

            if (_isFalling == isFalling) return;

            _isFalling = isFalling;
            Apply();
        }

        public void SetGrounded(bool isGrounded)
        {
            _isGrounded = isGrounded;

            if (isGrounded)
            {
                _isFalling = false;
                Apply();
            }
        }

        public void Disable()
        {
            _enabled = false;
            _rigidbody.gravityScale = 0f;
            CurrentGravity = 0f;
        }

        public void Enable()
        {
            _enabled = true;
            Apply();
        }

        private void Apply()
        {
            if (!_enabled) return;

            var gravity = _isFalling
                ? _stats.BaseGravityScale * _stats.FallGravityMultiplier
                : _stats.BaseGravityScale;

            _rigidbody.gravityScale = gravity;
            CurrentGravity = gravity;
        }
    }
}