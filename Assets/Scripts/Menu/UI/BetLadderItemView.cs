using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class BetLadderItemView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private GameObject selectedHighlight;
        [SerializeField] private string amountFormat = "0.##";

        private int _index;

        public event Action<int> OnPressed;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(HandleClick);
        }

        public void Render(int index, float amount, bool isSelected, bool isAffordable)
        {
            _index = index;

            if (amountText != null)
                amountText.text = amount.ToString(amountFormat);

            if (selectedHighlight != null)
                selectedHighlight.SetActive(isSelected);

            if (button != null)
                button.interactable = isAffordable && !isSelected;
        }

        private void HandleClick()
        {
            OnPressed?.Invoke(_index);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveAllListeners();
        }
    }
}
