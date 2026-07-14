using System.Collections;
using UnityEngine;
using Spine.Unity;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;

namespace SweetSweeps.Gameplay.Player
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SkeletonMecanim skeletonMecanim;

        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int DoubleJump = Animator.StringToHash("DoubleJump");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int DeathType = Animator.StringToHash("DeathType");
        
        [Header("Settings")]
        [SerializeField] private float deathDefaultDelay = 0.2f;
        [SerializeField] private float deathCalamityDelay = 1f;
        
        private bool _isDead;
        private float _inputDirectionX;
        
        private IHealthService _healthService;
        private IPhaseService _phaseService;
        
        public event System.Action<DamageType> OnDeathAnimationComplete;

        public void Initialize(IHealthService healthService, IPhaseService phaseService)
        {
            _healthService = healthService;
            _phaseService = phaseService;
            _healthService.OnDeath += OnPlayerDeath;
        }

        public void UpdateAnimation(
            bool isGrounded,
            bool justJumped,
            bool justDoubleJumped,
            float inputX)
        {
            if (_isDead) return;

            _inputDirectionX = inputX;
            HandleFacing(_inputDirectionX);
            animator.SetBool(IsGrounded, isGrounded);
            animator.SetFloat(InputX, Mathf.Abs(_inputDirectionX));
            
            if (justDoubleJumped)
            {
                animator.SetTrigger(DoubleJump);
                return;
            }
            
            if (justJumped)
            {
                animator.SetTrigger(Jump);
            }
        }

        private void OnPlayerDeath(DamageType damageType)
        {
            if (_isDead) return;
            _isDead = true;

            animator.SetBool(IsDead, true);
            animator.SetInteger(DeathType, ResolveDeathAnimation(damageType));

            if (isActiveAndEnabled) StartCoroutine(DeathCompleteRoutine(damageType));
        }
        
        private IEnumerator DeathCompleteRoutine(DamageType damageType)
        {
            var delay = damageType == DamageType.BiomeCalamity ? deathCalamityDelay : deathDefaultDelay;
            
            yield return new WaitForSeconds(delay);
            OnDeathAnimationComplete?.Invoke(damageType);
        }

        private int ResolveDeathAnimation(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Collectibles => 0,
                DamageType.BiomeCalamity => ResolveBiomeDeath(),
                DamageType.Fall => 0,
                _ => 0
            };
        }
        
        private int ResolveBiomeDeath()
        {
            if (_phaseService == null)
                return 0;

            string biomeId = _phaseService.BiomeCalamityId;

            return biomeId switch
            {
                "frost" => 2,
                "heavy" => 1,
                _ => 0
            };
        }

        private void HandleFacing(float inputX)
        {
            if (Mathf.Abs(inputX) < 0.01f) return;
            skeletonMecanim.Skeleton.ScaleX = inputX > 0f ? 1f : -1f;
        }
        
        public void ResetToIdle()
        {
            _isDead = false;
            _inputDirectionX = 0f;
            animator.SetBool(IsDead, false);
            animator.SetFloat(InputX, 0f);
            animator.SetBool(IsGrounded, true);
            skeletonMecanim.Skeleton.ScaleX = 1f;
            skeletonMecanim.Skeleton.A = 1f;
            animator.Play("Locomotion", 0, 0f);

        }

        private void OnDestroy()
        {
            if (_healthService != null)
                _healthService.OnDeath -= OnPlayerDeath;
        }
    }
}