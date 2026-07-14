using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Data;

namespace SweetSweeps.Gameplay.Player
{
    public class PlayerMover
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly PlayerStatsSO _stats;
        private readonly IMovementModifierRegistry _modifierRegistry;

        private float _currentVelocityX;

        public PlayerMover(
            Rigidbody2D rigidbody,
            PlayerStatsSO stats,
            IMovementModifierRegistry modifierRegistry)
        {
            _rigidbody = rigidbody;
            _stats = stats;
            _modifierRegistry = modifierRegistry;
        }

        public void Move(float horizontalInput, bool isGrounded, Rigidbody2D platformRigidbody = null)
        {
            MovementModifier modifier = _modifierRegistry.GetCombined();

            float platformVelocityX = platformRigidbody != null
                ? platformRigidbody.linearVelocity.x
                : 0f;

            float speed = isGrounded
                ? _stats.GroundMoveSpeed * modifier.SpeedMultiplier
                : _stats.AirMoveSpeed * modifier.SpeedMultiplier;

            float targetVelocityX = platformVelocityX + horizontalInput * speed;
            float localVelocityX = _rigidbody.linearVelocity.x - platformVelocityX;

            bool isAccelerating = Mathf.Abs(horizontalInput) > 0.01f;
            bool isChangingDirection = isAccelerating
                                       && Mathf.Sign(horizontalInput) != Mathf.Sign(localVelocityX)
                                       && Mathf.Abs(localVelocityX) > 0.1f;

            float acceleration;
            if (isAccelerating)
            {
                float baseAcceleration = isGrounded
                    ? _stats.GroundAcceleration
                    : _stats.AirAcceleration;

                float directionFactor = isChangingDirection
                    ? modifier.DirectionChangeMultiplier
                    : 1f;

                acceleration = baseAcceleration * modifier.AccelerationMultiplier / directionFactor;
            }
            else
            {
                float baseDeceleration = isGrounded
                    ? _stats.GroundDeceleration
                    : _stats.AirDeceleration;

                acceleration = baseDeceleration * modifier.DecelerationMultiplier;
            }

            float newLocalVelocityX = Mathf.MoveTowards(
                localVelocityX,
                targetVelocityX - platformVelocityX,
                acceleration * Time.deltaTime);

            _rigidbody.linearVelocity = new Vector2(
                newLocalVelocityX + platformVelocityX,
                _rigidbody.linearVelocity.y);
        }

        public void Stop()
        {
            _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);
        }
    }
}