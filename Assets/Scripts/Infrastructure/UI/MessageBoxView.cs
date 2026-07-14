using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSweeps.Infrastructure.UI
{
    public class MessageBoxView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        private UniTaskCompletionSource _completionSource;

        private void Awake()
        {
            panel.SetActive(false);
            confirmButton.onClick.AddListener(OnConfirm);
        }

        public UniTask ShowAsync(string title, string message, string confirmLabel = "OK")
        {
            titleText.text = title;
            messageText.text = message;
            confirmButtonText.text = confirmLabel;

            panel.SetActive(true);

            _completionSource = new UniTaskCompletionSource();
            return _completionSource.Task;
        }

        private void OnConfirm()
        {
            panel.SetActive(false);
            _completionSource?.TrySetResult();
        }

        private void OnDestroy()
        {
            confirmButton.onClick.RemoveAllListeners();
        }
    }
}
