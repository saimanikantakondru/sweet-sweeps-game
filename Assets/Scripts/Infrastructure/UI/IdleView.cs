using UnityEngine;
using UnityEngine.UI;

namespace SweetSweeps.Infrastructure.UI
{
    public class IdleView : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        public event System.Action OnStartPressed;

        private void Awake()
        {
            startButton.onClick.AddListener(() =>
            {
                Debug.Log("[IdleView] Start pressed.");
                OnStartPressed?.Invoke();
            });
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        private void OnDestroy()
        {
            startButton.onClick.RemoveAllListeners();
        }
    }
}