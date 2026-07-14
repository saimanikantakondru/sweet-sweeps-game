using DG.Tweening;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SweetSweeps.Infrastructure.UI
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private string defaultMessage = "Loading...";

        [SerializeField] private GameObject spinnerRoot;
        [SerializeField] private RectTransform spinnerIcon;
        [SerializeField, MinValue(0.01f)] private float spinnerRevolutionTime = 1f;

        private Tween _spinnerTween;

        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        public void Show(string message = null)
        {
            if (messageText != null)
                messageText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;

            if (panel != null)
                panel.SetActive(true);

            StartSpinner();
        }

        public void Hide()
        {
            StopSpinner();

            if (panel != null)
                panel.SetActive(false);
        }

        private void StartSpinner()
        {
            if (spinnerRoot != null) spinnerRoot.SetActive(true);
            if (spinnerIcon == null) return;

            _spinnerTween?.Kill();
            _spinnerTween = spinnerIcon
                .DOLocalRotate(new Vector3(0f, 0f, -360f), spinnerRevolutionTime, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        private void StopSpinner()
        {
            _spinnerTween?.Kill();
            _spinnerTween = null;
            if (spinnerRoot != null) spinnerRoot.SetActive(false);
        }
    }
}
