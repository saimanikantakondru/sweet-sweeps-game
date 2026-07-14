using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Infrastructure.UI
{
    public class GameOverView : MonoBehaviour
    {
        [Title("Coin Counts")]
        [SerializeField] private TextMeshProUGUI goldCoinsTextShadow;
        [SerializeField] private TextMeshProUGUI goldCoinsText;
        [SerializeField] private TextMeshProUGUI ssCoinsTextShadow;
        [SerializeField] private TextMeshProUGUI ssCoinsText;
        [SerializeField] private TextMeshProUGUI totalCoinsText;

        [Title("Pending")]
        [SerializeField] private GameObject spinnerRoot;
        [SerializeField] private RectTransform spinnerIcon;
        [SerializeField, MinValue(0.01f)] private float spinnerRevolutionTime = 1f;

        [Title("Buttons")]
        [SerializeField] private GameObject actionButtonsRoot;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [Title("Banners")]
        [SerializeField] private GameObject levelCompleteBanner;
        [SerializeField] private GameObject jackpotBanner;
        [SerializeField] private float jackpotPunchScale = 0.3f;
        [SerializeField] private float jackpotPunchDuration = 0.6f;

        [Title("Backdrop")]
        [SerializeField] private CanvasGroup backdropCanvasGroup;
        [SerializeField, MinValue(0f)] private float backdropFadeDuration = 0.35f;

        [Title("Window")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, MinValue(0f)] private float fadeDuration = 0.4f;

        private const string PendingPlaceholder = "...";
        private const string FailedPlaceholder = "—";

        private Coroutine _fadeRoutine;
        private Tween _spinnerTween;

        public bool ResultResolved { get; private set; }

        public event Action OnRestartPressed;
        public event Action OnMainMenuPressed;

        private void Awake()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(HandlePlayAgain);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(HandleMainMenu);
        }

        private void OnDestroy()
        {
            if (playAgainButton != null) playAgainButton.onClick.RemoveAllListeners();
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        }

        public void Show(CollectionReport report, bool awaitResult)
        {
            gameObject.SetActive(true);
            ResultResolved = false;
            
            int gold = report?.goldCoinsCollected ?? 0;
            int ssCoins = report?.purpleCoinsCollected ?? 0;
            int total = gold + ssCoins;
            if (totalCoinsText != null) totalCoinsText.text = total.ToString();
            
            HideOutcomeBanners();

            if (awaitResult)
            {
                EnterPending();
            }
            else
            {
                ShowOutcomeBanner(false);
                Resolve();
            }

            PlayFadeIn();

            Debug.Log($"[GameOverView] Shown. AwaitResult={awaitResult}");
        }

        public void SetWinResult(float totalWin, float goldScore, string currency, bool jackpotAwarded)
        {
            StopSpinner();

            SetWinTexts(FormatWin(totalWin, currency));
            SetScoreTexts(goldScore.ToString("0.##"));

            ShowOutcomeBanner(jackpotAwarded);

            Resolve();
            Debug.Log($"[GameOverView] Win set. TotalWin={totalWin} Score={goldScore} Jackpot={jackpotAwarded}");
        }

        public void SetWinFailed()
        {
            StopSpinner();

            SetWinTexts(FailedPlaceholder);
            SetScoreTexts(FailedPlaceholder);

            ShowOutcomeBanner(false);
            Resolve();
            Debug.LogWarning("[GameOverView] Win result failed.");
        }

        public void Hide()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            if (backdropCanvasGroup != null)
            {
                backdropCanvasGroup.alpha = 0f;
                backdropCanvasGroup.blocksRaycasts = false;
            }

            StopSpinner();
            gameObject.SetActive(false);
        }

        private void EnterPending()
        {
            SetWinTexts(PendingPlaceholder);
            SetScoreTexts(PendingPlaceholder);

            if (actionButtonsRoot != null) actionButtonsRoot.SetActive(false);
            StartSpinner();
        }

        private void SetWinTexts(string value)
        {
            if (ssCoinsTextShadow != null) ssCoinsTextShadow.text = value;
            if (ssCoinsText != null) ssCoinsText.text = value;
        }

        private void SetScoreTexts(string value)
        {
            if (goldCoinsTextShadow != null) goldCoinsTextShadow.text = value;
            if (goldCoinsText != null) goldCoinsText.text = value;
        }

        private void Resolve()
        {
            if (actionButtonsRoot != null) actionButtonsRoot.SetActive(true);
            ResultResolved = true;
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

        private void ShowJackpot()
        {
            if (jackpotBanner == null) return;

            jackpotBanner.SetActive(true);
            jackpotBanner.transform.localScale = Vector3.one;
            jackpotBanner.transform
                .DOPunchScale(Vector3.one * jackpotPunchScale, jackpotPunchDuration)
                .SetUpdate(true)
                .SetLink(jackpotBanner);
        }

        private void HideOutcomeBanners()
        {
            if (levelCompleteBanner != null) levelCompleteBanner.SetActive(false);
            if (jackpotBanner != null) jackpotBanner.SetActive(false);
        }

        private void ShowOutcomeBanner(bool jackpot)
        {
            if (jackpot)
            {
                if (levelCompleteBanner != null) levelCompleteBanner.SetActive(false);
                ShowJackpot();
            }
            else
            {
                if (jackpotBanner != null) jackpotBanner.SetActive(false);
                if (levelCompleteBanner != null) levelCompleteBanner.SetActive(true);
            }
        }

        private static string FormatWin(float totalWin, string currency)
        {
            return string.IsNullOrEmpty(currency)
                ? totalWin.ToString("0.00")
                : $"{totalWin:0.00}";
        }

        private void PlayFadeIn()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (backdropCanvasGroup != null)
            {
                backdropCanvasGroup.alpha = 0f;
                backdropCanvasGroup.blocksRaycasts = true;
                yield return FadeAlpha(backdropCanvasGroup, backdropFadeDuration);
            }

            if (canvasGroup != null)
            {
                yield return FadeAlpha(canvasGroup, fadeDuration);
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            _fadeRoutine = null;
        }

        private static IEnumerator FadeAlpha(CanvasGroup group, float duration)
        {
            if (duration <= 0f)
            {
                group.alpha = 1f;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            group.alpha = 1f;
        }

        private void HandlePlayAgain()
        {
            Debug.Log("[GameOverView] Play Again pressed.");
            OnRestartPressed?.Invoke();
        }

        private void HandleMainMenu()
        {
            Debug.Log("[GameOverView] Main Menu pressed.");
            OnMainMenuPressed?.Invoke();
        }
    }
}
