using System;
using UnityEngine;
using UnityEngine.UI;

namespace SweetSweeps.Infrastructure.UI
{
    public class DebugNetworkPanelView : MonoBehaviour
    {
        [SerializeField] private Button simulateConnectionLostButton;
        [SerializeField] private Button simulateSilentDropButton;

        public event Action OnSimulateConnectionLost;
        public event Action OnSimulateSilentDrop;

        private void Awake()
        {
            if (simulateConnectionLostButton != null)
                simulateConnectionLostButton.onClick.AddListener(HandleConnectionLost);

            if (simulateSilentDropButton != null)
                simulateSilentDropButton.onClick.AddListener(HandleSilentDrop);
        }

        private void OnDestroy()
        {
            if (simulateConnectionLostButton != null)
                simulateConnectionLostButton.onClick.RemoveAllListeners();

            if (simulateSilentDropButton != null)
                simulateSilentDropButton.onClick.RemoveAllListeners();
        }

        private void HandleConnectionLost() => OnSimulateConnectionLost?.Invoke();

        private void HandleSilentDrop() => OnSimulateSilentDrop?.Invoke();
    }
}
