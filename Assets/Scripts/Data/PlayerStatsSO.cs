using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "SweetSweeps/PlayerStats")]
    public class PlayerStatsSO : ScriptableObject
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 1;

        [Header("Movement")]
        [SerializeField] private float groundMoveSpeed = 6f;
        [SerializeField] private float airMoveSpeed = 5f;

        [Header("Acceleration")]
        [SerializeField] private float groundAcceleration = 15f;
        [SerializeField] private float groundDeceleration = 20f;
        [SerializeField] private float airAcceleration = 10f;
        [SerializeField] private float airDeceleration = 8f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private bool allowDoubleJump = false;

        [Header("Gravity")]
        [SerializeField] private float baseGravityScale = 3f;
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [Header("Platform Drop")]
        [SerializeField] private float dropThroughForce = 3f;

        [Header("Platforming Assistance")]
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        
        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 1f;
        [SerializeField] private float respawnInvincibilityDuration = 3f;
        
        [Header("Invincibility")]
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private bool allowKnockback = false;

        public int MaxHealth => maxHealth;
        public float GroundMoveSpeed => groundMoveSpeed;
        public float AirMoveSpeed => airMoveSpeed;
        public float GroundAcceleration => groundAcceleration;
        public float GroundDeceleration => groundDeceleration;
        public float AirAcceleration => airAcceleration;
        public float AirDeceleration => airDeceleration;
        public float JumpForce => jumpForce;
        public bool AllowDoubleJump => allowDoubleJump;
        public float BaseGravityScale => baseGravityScale;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float DropThroughForce => dropThroughForce;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float InvincibilityDuration => invincibilityDuration;
        public float RespawnInvincibilityDuration => respawnInvincibilityDuration;
        public float KnockbackForce => knockbackForce;
        public bool AllowKnockback => allowKnockback;

        // TODO: Encapsulate this
        public void SetAllowDoubleJump(bool value) => allowDoubleJump = value;
        public void SetAllowKnockback(bool value) => allowKnockback = value;
        // public void SetGroundMoveSpeed(float value) => groundMoveSpeed = value;
        // public void SetAirMoveSpeed(float value) => airMoveSpeed = value;
        // public void SetJumpForce(float value) => jumpForce = value;
        // public void SetFallGravityMultiplier(float value) => fallGravityMultiplier = value;
        // public void SetGroundAcceleration(float value) => groundAcceleration = value;
        // public void SetGroundDeceleration(float value) => groundDeceleration = value;
        // public void SetAirAcceleration(float value) => airAcceleration = value;
        // public void SetAirDeceleration(float value) => airDeceleration = value;
    }
}