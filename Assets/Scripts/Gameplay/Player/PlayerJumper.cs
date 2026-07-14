using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Player
{
    public class PlayerJumper
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly PlayerStatsSO _stats;
        private readonly IPlayerGravityService _gravityService;
        private readonly IMovementModifierRegistry _modifierRegistry;

        private bool _isGrounded;
        private bool _hasDoubleJump;
        private float _coyoteTimeCounter;
        private bool _isCoyoteAvailable;
        private float _jumpBufferCounter;

        public PlayerJumper(
            Rigidbody2D rigidbody,
            PlayerStatsSO stats,
            IPlayerGravityService gravityService,
            IMovementModifierRegistry modifierRegistry)
        {
            _rigidbody = rigidbody;
            _stats = stats;
            _gravityService = gravityService;
            _modifierRegistry = modifierRegistry;
        }

        public int OnLanded()
        {
            _isGrounded        = true;
            _isCoyoteAvailable = true;
            _coyoteTimeCounter = _stats.CoyoteTime;
            _hasDoubleJump     = _stats.AllowDoubleJump;
            _gravityService.SetGrounded(true);

            if (_jumpBufferCounter > 0f)
            {
                _jumpBufferCounter = 0f;
                ExecuteJump();
                return 1;
            }

            return 0;
        }

        public void OnLeftGround()
        {
            _isGrounded = false;
            _gravityService.SetGrounded(false);
            //Debug.Log("[PlayerJumper] Left ground.");
        }

        // Returns: 0 = no jump, 1 = normal jump, 2 = double jump
        public int TryJump()
        {
            if (_isGrounded)
            {
                ExecuteJump();
                return 1;
            }

            if (_isCoyoteAvailable && _coyoteTimeCounter > 0f)
            {
                _isCoyoteAvailable = false;
                ExecuteJump();
                //Debug.Log("[PlayerJumper] Coyote jump.");
                return 1;
            }

            if (_hasDoubleJump)
            {
                _hasDoubleJump = false;
                ExecuteJump();
                //Debug.Log("[PlayerJumper] Double jump.");
                return 2;
            }

            _jumpBufferCounter = _stats.JumpBufferTime;
            return 0;
        }

        public void UpdateAirState(float deltaTime)
        {
            if (!_isGrounded)
            {
                if (_coyoteTimeCounter > 0f)
                    _coyoteTimeCounter -= deltaTime;

                _gravityService.SetFalling(_rigidbody.linearVelocity.y < -0.01f);
            }

            if (_jumpBufferCounter > 0f)
                _jumpBufferCounter -= deltaTime;
        }

        private void ExecuteJump()
        {
            MovementModifier modifier = _modifierRegistry.GetCombined();
            float jumpForce = _stats.JumpForce * modifier.JumpForceMultiplier;

            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, 0f);
            _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _isGrounded = false;
            _isCoyoteAvailable = false;
            _coyoteTimeCounter = 0f;
            _gravityService.SetFalling(false);
            //Debug.Log($"[PlayerJumper] Jump executed. force={jumpForce:F2}");
        }
    }
}