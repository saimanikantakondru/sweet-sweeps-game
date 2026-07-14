using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class InfoPopupView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;
        [SerializeField] private ScrollRect bodyScrollRect;

        [Header("Content")]
        [SerializeField] private string defaultTitle = "How to Play";
        [SerializeField, TextArea(6, 20)] private string defaultBody;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (closeButton    != null) closeButton.onClick.AddListener(Close);
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);

            Close();
        }

        public void Open()
        {
            Open(defaultTitle, defaultBody);
        }

        public void Open(string title, string body)
        {
            if (titleText != null) titleText.text = title;
            if (bodyText  != null) bodyText.text  = body;
            if (root      != null) root.SetActive(true);

            ResetScroll();
        }

        private void ResetScroll()
        {
            if (bodyScrollRect == null) return;

            Canvas.ForceUpdateCanvases();
            bodyScrollRect.velocity = Vector2.zero;
            bodyScrollRect.verticalNormalizedPosition = 1f;
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (closeButton    != null) closeButton.onClick.RemoveAllListeners();
            if (backdropButton != null) backdropButton.onClick.RemoveAllListeners();
        }
    }
}
