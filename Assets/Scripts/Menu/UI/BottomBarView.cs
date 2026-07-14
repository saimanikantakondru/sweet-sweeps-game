using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class BottomBarView : MonoBehaviour
    {
        [Header("Children")]
        [SerializeField] private BetSelectorView betSelectorView;

        [Header("Play")]
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI playLabel;
        [SerializeField] private string playLabelDefault = "CONFIRM BET";
        [SerializeField] private string playLabelInsufficient = "INSUFFICIENT";
        [SerializeField] private string playLabelResume = "RESUME";

        private bool _resumeMode;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI feedbackText;

        public BetSelectorView BetSelector => betSelectorView;

        public event Action OnPlayPressed;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(() => OnPlayPressed?.Invoke());
        }

        public void SetPlayInteractable(bool interactable)
        {
            if (playButton != null) playButton.interactable = interactable;
        }

        public void SetPlayBusy(bool busy)
        {
            if (playButton != null) playButton.interactable = !busy;
        }

        public void SetInsufficientBalance(bool insufficient)
        {
            if (_resumeMode) return;
            if (playLabel == null) return;
            playLabel.text = insufficient ? playLabelInsufficient : playLabelDefault;
        }

        public void SetResumeMode(bool resume)
        {
            _resumeMode = resume;
            if (resume && playLabel != null) playLabel.text = playLabelResume;
        }

        public void SetFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message ?? string.Empty;
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveAllListeners();
        }
    }
}
