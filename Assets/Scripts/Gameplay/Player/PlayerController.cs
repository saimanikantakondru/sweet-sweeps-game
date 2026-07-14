using System;
using UnityEngine;
using VContainer;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Level;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask platformLayer;

        private Rigidbody2D _rigidbody;
        private Rigidbody2D _currentPlatformRigidbody;
        private IPlayerGravityService _gravityService;
        private int _platformLayerIndex;

        private IInputProvider _input;
        private IHealthService _healthService;
        private IGameStateMachine _stateMachine;
        private PlayerStatsSO _stats;

        private PlayerMover _mover;
        private PlayerJumper _jumper;
        private PlayerAnimatorController _animController;
        private CapsuleCollider2D _capsuleCollider;

        private const float WalkInputEpsilon = 0.01f;
        private const float WalkSpeedEpsilon = 0.05f;

        private bool _isActive;
        private bool _wasGrounded;
        private bool _justJumped;
        private bool _justDoubleJumped;
        private bool _isDropping;
        private bool _isWalking;

        public bool IsGrounded { get; private set; }
        public Vector2 Velocity => _rigidbody.linearVelocity;

        public event Action OnJumped;
        public event Action OnDoubleJumped;
        public event Action<bool> OnWalkStateChanged;

        [Inject]
        public void Construct(
            IInputProvider input,
            IHealthService healthService,
            IGameStateMachine stateMachine,
            ICollectibleService collectibleService,
            IMovementModifierRegistry modifierRegistry,
            IFrameService iFrameService,
            IPhaseService phaseService,
            PlayerStatsSO stats)
        {
            _input = input;
            _healthService = healthService;
            _stateMachine = stateMachine;
            _stats = stats;

            _rigidbody = GetComponent<Rigidbody2D>();
            _platformLayerIndex = ToLayerIndex(platformLayer);
            _gravityService = new PlayerGravityService(_rigidbody, stats);

            _mover = new PlayerMover(_rigidbody, stats, modifierRegistry);
            _jumper = new PlayerJumper(_rigidbody, stats, _gravityService, modifierRegistry);

            var knockbackService = new KnockbackService(_rigidbody, stats);
            var trigger = GetComponentInChildren<PlayerContactTrigger>();
            trigger.Initialize(healthService, collectibleService, knockbackService);
            
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            _animController = GetComponentInChildren<PlayerAnimatorController>();
            if (_animController != null)
            {
                _animController.Initialize(healthService, phaseService);
                _animController.OnDeathAnimationComplete += OnDeathAnimationComplete;
            }
            
            var visualEffect = GetComponentInChildren<PlayerVisualIFrameEffect>();
            if (visualEffect != null)
                visualEffect.Initialize(iFrameService);

            var stickyFeet = GetComponentInChildren<PlayerStickyFeetVfx>();
            if (stickyFeet != null)
                stickyFeet.Initialize(phaseService, this);

            _healthService.OnDeath += OnPlayerDeath;
            //Debug.Log("[PlayerController] Initialized.");
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
            {
                _mover.Stop();
                SetWalking(false);
            }
            else
            {
                Unfreeze();
                _animController?.ResetToIdle();
            }
        }

        public void Freeze()
        {
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
            _rigidbody.simulated = false;
        }

        public void Unfreeze()
        {
            _rigidbody.simulated = true;
        }

        private void Update()
        {
            if (!_isActive) return;

            _input.Poll();
            _justJumped = false;
            _justDoubleJumped = false;

            CheckGround();
            HandleMovement();
            HandleJump();
            HandleDropThrough();
            HandleDropThroughRecovery();
            _jumper.UpdateAirState(Time.deltaTime);
            
            _animController?.UpdateAnimation(
                IsGrounded,
                _justJumped,
                _justDoubleJumped,
                _input.MovementInput.x);

            UpdateWalkState();
        }

        private void CheckGround()
        {
            int combinedMask = groundLayer | platformLayer;

            IsGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position,
                groundCheckRadius,
                combinedMask);

            if (IsGrounded && !_wasGrounded)
                _justJumped = _jumper.OnLanded() == 1;
            else if (!IsGrounded && _wasGrounded)
                _jumper.OnLeftGround();

            _wasGrounded = IsGrounded;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<PlatformMover>(out var platform))
                _currentPlatformRigidbody = platform.PlatformRigidbody;
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            if (col.gameObject.TryGetComponent<PlatformMover>(out _))
                _currentPlatformRigidbody = null;
        }

        private void HandleMovement()
        {
            _mover.Move(_input.MovementInput.x, IsGrounded, _currentPlatformRigidbody);
        }

        private void HandleJump()
        {
            if (!_input.JumpPressed) return;

            int result = _jumper.TryJump();

            if (result == 1)
            {
                _justJumped = true;
                OnJumped?.Invoke();
            }
            else if (result == 2)
            {
                _justDoubleJumped = true;
                OnDoubleJumped?.Invoke();
            }
        }

        private void UpdateWalkState()
        {
            bool walking = IsGrounded
                && Mathf.Abs(_input.MovementInput.x) > WalkInputEpsilon
                && Mathf.Abs(Velocity.x) > WalkSpeedEpsilon;

            SetWalking(walking);
        }

        private void SetWalking(bool walking)
        {
            if (walking == _isWalking) return;
            _isWalking = walking;
            OnWalkStateChanged?.Invoke(walking);
        }

        private void HandleDropThrough()
        {
            if (_isDropping) return;
            if (!_input.DropPressed || !IsGrounded) return;

            _isDropping = true;

            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                _platformLayerIndex,
                true);

            _rigidbody.AddForce(Vector2.down * _stats.DropThroughForce, ForceMode2D.Impulse);
            //Debug.Log("[PlayerController] Drop through platform.");
        }

        private void HandleDropThroughRecovery()
        {
            if (!_isDropping) return;
            if (_input.DropPressed) return;
            if (IsGrounded) return;

            RestorePlatformCollision();
        }

        private void RestorePlatformCollision()
        {
            _isDropping = false;
            Physics2D.IgnoreLayerCollision(
                gameObject.layer,
                _platformLayerIndex,
                false);
        }

        private void OnPlayerDeath(DamageType damageType)
        {
            SetActive(false);
            Freeze();
        }
        
        private void OnDeathAnimationComplete(DamageType damageType)
        {
            _stateMachine.ChangeState<GameOverState>();
            Debug.Log("[PlayerController] Death animation complete — GameOver.");
        }
        
        public void ResetPhysics()
        {
            Unfreeze();

            if (_capsuleCollider != null)
                _capsuleCollider.enabled = true;

            _gravityService.Enable();
            _animController?.ResetToIdle();
        }

        private void OnDestroy()
        {
            if (_healthService != null)
                _healthService.OnDeath -= OnPlayerDeath;
        }

        private static int ToLayerIndex(LayerMask mask)
        {
            int value = mask.value;
            for (int i = 0; i < 32; i++)
            {
                if ((value & (1 << i)) != 0)
                    return i;
            }
            return 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}