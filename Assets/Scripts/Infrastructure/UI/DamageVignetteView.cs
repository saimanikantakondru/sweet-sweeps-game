using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSweeps.Infrastructure.UI
{
    public class DamageVignetteView : MonoBehaviour
    {
        [Title("Refs")]
        [SerializeField, Required] private Image vignetteImage;

        [Title("Tuning")]
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.6f;
        [SerializeField] private float fadeInDuration = 0.08f;
        [SerializeField] private float fadeOutDuration = 0.35f;

        private Sequence _flashSequence;

        private void Awake()
        {
            SetAlpha(0f);
        }

        public void Flash()
        {
            _flashSequence?.Kill();
            SetAlpha(0f);

            _flashSequence = DOTween.Sequence()
                .Append(vignetteImage.DOFade(maxAlpha, fadeInDuration))
                .Append(vignetteImage.DOFade(0f, fadeOutDuration))
                .SetLink(gameObject);
        }

        private void SetAlpha(float a)
        {
            var color = vignetteImage.color;
            color.a = a;
            vignetteImage.color = color;
        }

        private void OnDestroy()
        {
            _flashSequence?.Kill();
        }
    }
}
