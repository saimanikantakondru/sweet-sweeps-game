using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine.States;

namespace SweetSweeps.Gameplay.Player
{
    public class PitDetector : MonoBehaviour
    {
        [SerializeField] private float killBelowY = -15f;
        
        private IHealthService _healthService;
        private GameSettingsSO _gameSettings;
        private IGameStateMachine _stateMachine;
        private bool _initialized;

        public void Initialize(IHealthService healthService, GameSettingsSO gameSettings, IGameStateMachine stateMachine)
        {
            _healthService = healthService;
            _gameSettings = gameSettings;
            _stateMachine = stateMachine;
            _initialized = true;
        }
        
        private void Update()
        {
            if (!_initialized || _healthService.IsDead) return;
            
            if (transform.position.y < killBelowY)
            {
                Debug.Log("[PitDetector] Kill Y threshold reached.");
                Kill();
            }
        }

        // Create bug when camera too slow on Y axis
        // private void OnBecameInvisible()
        // {
        //     if (!_initialized || _healthService.IsDead) return;
        //     if (_stateMachine.CurrentState is not PlayingState) return;
        //
        //     Debug.Log("[PitDetector] Kill player OnBecameInvisible.");
        //     Kill();
        // }

        private void Kill()
        {
            _healthService.TakeDamage(_gameSettings.PitDamage, DamageType.Fall);
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(-100f, killBelowY, 0f),
                new Vector3(100f, killBelowY, 0f));
        }
    }
}