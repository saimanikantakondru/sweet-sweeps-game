using System;
using System.Collections;
using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class InvincibilityService : IFrameService
    {
        private readonly PlayerStatsSO _stats;
        private readonly MonoBehaviour _coroutineRunner;

        private Coroutine _activeCoroutine;

        public bool IsInvincible { get; private set; }

        public event Action OnInvincibilityStarted;
        public event Action OnInvincibilityEnded;

        public InvincibilityService(PlayerStatsSO stats, MonoBehaviour coroutineRunner)
        {
            _stats = stats;
            _coroutineRunner = coroutineRunner;
        }

        public void TriggerInvincibility(bool isRespawn = false)
        {
            if (IsInvincible && !isRespawn) return;

            if (_activeCoroutine != null)
                _coroutineRunner.StopCoroutine(_activeCoroutine);

            _activeCoroutine = _coroutineRunner.StartCoroutine(InvincibilityRoutine(isRespawn));
        }

        private IEnumerator InvincibilityRoutine(bool isRespawn)
        {
            IsInvincible = true;
            OnInvincibilityStarted?.Invoke();
            //Debug.Log($"[InvincibilityService] Invincible for {_stats.InvincibilityDuration}s");

            if (isRespawn)
            {
                yield return new WaitForSeconds(_stats.RespawnInvincibilityDuration);

            }
            else
            {
                yield return new WaitForSeconds(_stats.InvincibilityDuration);
            }

            IsInvincible = false;
            _activeCoroutine = null;
            OnInvincibilityEnded?.Invoke();
            //Debug.Log("[InvincibilityService] Invincibility ended.");
        }
    }
}