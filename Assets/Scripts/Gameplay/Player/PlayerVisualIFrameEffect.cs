using System.Collections;
using UnityEngine;
using Spine.Unity;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Player
{
    public class PlayerVisualIFrameEffect : MonoBehaviour
    {
        [SerializeField] private SkeletonMecanim skeletonMecanim;
        [SerializeField] private float blinkInterval = 0.1f;

        private Coroutine _blinkCoroutine;

        public void Initialize(IFrameService iFrameService)
        {
            iFrameService.OnInvincibilityStarted += StartBlink;
            iFrameService.OnInvincibilityEnded += StopBlink;
        }

        private void StartBlink()
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);

            _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        private void StopBlink()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }

            skeletonMecanim.Skeleton.A = 1f;
        }

        private IEnumerator BlinkRoutine()
        {
            bool visible = true;

            while (true)
            {
                visible = !visible;
                skeletonMecanim.Skeleton.A = visible ? 1f : 0f;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private void OnDestroy()
        {
            StopBlink();
        }
    }
}