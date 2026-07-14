using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SweetSweeps.Infrastructure.UI
{
    [RequireComponent(typeof(TextMeshPro))]
    public class CoinValuePopupView : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshPro label;
        [SerializeField] private string valueFormat = "0.##";
        [SerializeField, MinValue(0f)] private float riseDistance = 1.2f;
        [SerializeField, MinValue(0.01f)] private float duration = 0.9f;
        [SerializeField] private Ease moveEase = Ease.OutCubic;

        private Tween _tween;
        private Transform _transform;
        private Action<CoinValuePopupView> _onComplete;

        private void Awake()
        {
            _transform = transform;
            if (label == null)
                label = GetComponent<TextMeshPro>();
        }

        public void Play(float value, Vector3 worldPosition, Action<CoinValuePopupView> onComplete)
        {
            _onComplete = onComplete;
            _transform.position = worldPosition;
            label.text = value.ToString(valueFormat);
            label.alpha = 1f;
            gameObject.SetActive(true);

            _tween?.Kill();
            _tween = DOTween.Sequence()
                .Append(_transform.DOMoveY(worldPosition.y + riseDistance, duration).SetEase(moveEase))
                .Join(DOTween.To(() => label.alpha, a => label.alpha = a, 0f, duration).SetEase(Ease.InQuad))
                .OnComplete(HandleComplete)
                .SetLink(gameObject);
        }

        public void StopAndHide()
        {
            _tween?.Kill();
            _tween = null;
            gameObject.SetActive(false);
        }

        private void HandleComplete()
        {
            _tween = null;
            _onComplete?.Invoke(this);
        }
    }
}
