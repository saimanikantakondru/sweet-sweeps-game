using System;
using UnityEngine;
using UnityEngine.UI;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Menu.UI
{
    public class CurrencySelectorView : MonoBehaviour
    {
        [SerializeField] private Button sweepsButton;
        [SerializeField] private Button goldButton;

        public event Action<string> OnCurrencySelected;

        private void Awake()
        {
            if (sweepsButton != null)
                sweepsButton.onClick.AddListener(() => OnCurrencySelected?.Invoke(CurrencyCodes.Sweeps));

            if (goldButton != null)
                goldButton.onClick.AddListener(() => OnCurrencySelected?.Invoke(CurrencyCodes.Gold));
        }

        public void Refresh(string activeCurrency, bool interactable)
        {
            if (sweepsButton != null)
                sweepsButton.interactable = interactable && activeCurrency != CurrencyCodes.Sweeps;

            if (goldButton != null)
                goldButton.interactable = interactable && activeCurrency != CurrencyCodes.Gold;
        }

        private void OnDestroy()
        {
            if (sweepsButton != null) sweepsButton.onClick.RemoveAllListeners();
            if (goldButton != null) goldButton.onClick.RemoveAllListeners();
        }
    }
}
