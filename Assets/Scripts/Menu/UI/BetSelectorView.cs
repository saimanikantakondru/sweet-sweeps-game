using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class BetSelectorView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI betAmountText;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private Button incrementButton;
        [SerializeField] private Button decrementButton;
        [SerializeField] private Button minButton;
        [SerializeField] private Button maxButton;
        [SerializeField] private Button ladderButton;

        public event Action OnIncrement;
        public event Action OnDecrement;
        public event Action OnMinPressed;
        public event Action OnMaxPressed;
        public event Action OnLadderPressed;

        private void Awake()
        {
            if (incrementButton != null) incrementButton.onClick.AddListener(() => OnIncrement?.Invoke());
            if (decrementButton != null) decrementButton.onClick.AddListener(() => OnDecrement?.Invoke());
            if (minButton       != null) minButton.onClick.AddListener(()       => OnMinPressed?.Invoke());
            if (maxButton       != null) maxButton.onClick.AddListener(()       => OnMaxPressed?.Invoke());
            if (ladderButton    != null) ladderButton.onClick.AddListener(()    => OnLadderPressed?.Invoke());
        }

        public void Render(float amount, bool canIncrement, bool canDecrement)
        {
            betAmountText.text = amount.ToString("0.##");

            if (incrementButton != null) incrementButton.interactable = canIncrement;
            if (decrementButton != null) decrementButton.interactable = canDecrement;
            if (minButton       != null) minButton.interactable       = canDecrement;
            if (maxButton       != null) maxButton.interactable       = canIncrement;
            if (ladderButton    != null) ladderButton.interactable    = true;
        }

        public void RenderLocked(float amount)
        {
            betAmountText.text = amount.ToString("0.##");
            SetInteractable(false);
        }

        public void SetCurrencyIcon(Sprite icon)
        {
            if (currencyIcon == null) return;

            currencyIcon.sprite = icon;
            currencyIcon.enabled = icon != null;
        }

        public void SetInteractable(bool interactable)
        {
            if (incrementButton != null) incrementButton.interactable = interactable;
            if (decrementButton != null) decrementButton.interactable = interactable;
            if (minButton       != null) minButton.interactable       = interactable;
            if (maxButton       != null) maxButton.interactable       = interactable;
            if (ladderButton    != null) ladderButton.interactable    = interactable;
        }

        private void OnDestroy()
        {
            if (incrementButton != null) incrementButton.onClick.RemoveAllListeners();
            if (decrementButton != null) decrementButton.onClick.RemoveAllListeners();
            if (minButton       != null) minButton.onClick.RemoveAllListeners();
            if (maxButton       != null) maxButton.onClick.RemoveAllListeners();
            if (ladderButton    != null) ladderButton.onClick.RemoveAllListeners();
        }
    }
}