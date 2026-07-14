using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class TopBarView : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Children")]
        [SerializeField] private BalanceView balanceView;
        [SerializeField] private BalanceView goldBalanceView;
        [SerializeField] private ClockView clockView;

        [Header("Buttons")]
        [SerializeField] private Button fullscreenButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button infoButton;
        [SerializeField] private Button menuButton;

        [Header("Icons")]
        [SerializeField] private GameObject soundOnIcon;
        [SerializeField] private GameObject soundOffIcon;
        [SerializeField] private GameObject fullscreenEnterIcon;
        [SerializeField] private GameObject fullscreenExitIcon;

        public BalanceView Balance => balanceView;
        public BalanceView GoldBalance => goldBalanceView;

        public event Action OnFullscreenPressed;
        public event Action OnSoundPressed;
        public event Action OnInfoPressed;
        public event Action OnMenuPressed;

        private void Awake()
        {
            if (fullscreenButton != null) fullscreenButton.onClick.AddListener(() => OnFullscreenPressed?.Invoke());
            if (soundButton      != null) soundButton.onClick.AddListener(()      => OnSoundPressed?.Invoke());
            if (infoButton       != null) infoButton.onClick.AddListener(()       => OnInfoPressed?.Invoke());
            if (menuButton       != null) menuButton.onClick.AddListener(()       => OnMenuPressed?.Invoke());
        }

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }

        public void SetMuted(bool muted)
        {
            if (soundOnIcon  != null) soundOnIcon.SetActive(!muted);
            if (soundOffIcon != null) soundOffIcon.SetActive(muted);
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (fullscreenEnterIcon != null) fullscreenEnterIcon.SetActive(!fullscreen);
            if (fullscreenExitIcon  != null) fullscreenExitIcon.SetActive(fullscreen);
        }

        // removed by client
        // public void SetFullscreenSupported(bool supported)
        // {
        //     if (fullscreenButton != null) fullscreenButton.gameObject.SetActive(supported);
        // }

        private void OnDestroy()
        {
            if (fullscreenButton != null) fullscreenButton.onClick.RemoveAllListeners();
            if (soundButton      != null) soundButton.onClick.RemoveAllListeners();
            if (infoButton       != null) infoButton.onClick.RemoveAllListeners();
            if (menuButton       != null) menuButton.onClick.RemoveAllListeners();
        }
    }
}
