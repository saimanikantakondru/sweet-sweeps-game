using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SweetSweeps.Infrastructure.UI
{
    public class HUDView : MonoBehaviour
    {
        [Title("Counters")]
        [SerializeField, Required] private TextMeshProUGUI goldCoinsText;
        [SerializeField, Required] private TextMeshProUGUI ssCoinsText;
        [SerializeField, Required] private TextMeshProUGUI timerText;

        [Title("Hearts")]
        [SerializeField, Required] private Image[] hearts;

        [Title("Heart Loss")]
        [SerializeField, MinValue(1)] private int heartLossBlinkCount = 3;
        [SerializeField] private float heartLossBlinkInterval = 0.1f;

        [Title("Timer Colors")]
        [SerializeField] private Color defaultTimerColor = Color.white;
        [SerializeField] private Color endingTimerColor = Color.red;

        private readonly Dictionary<int, Tween> _lossTweens = new();
        private int _currentHealth = -1;

        public void SetGoldCoins(float value)
        {
            float displayValue = Mathf.Floor(value * 100f) / 100f;
            goldCoinsText.text = displayValue.ToString("0.##");
        }

        public void SetSsCoins(int amount)
        {
            ssCoinsText.text = amount.ToString();
        }

        public void SetTime(float seconds)
        {
            int s = Mathf.CeilToInt(seconds);
            timerText.text = s.ToString();
            timerText.color = s <= 10 ? endingTimerColor : defaultTimerColor;
        }

        public void SetHealth(int current, int max)
        {
            int previous = _currentHealth;
            _currentHealth = current;
            bool isInitial = previous < 0;

            for (int i = 0; i < hearts.Length; i++)
            {
                var heart = hearts[i];
                if (heart == null) continue;

                bool shouldBeActive = i < current;
                bool wasActive = !isInitial && i < previous;

                if (shouldBeActive)
                {
                    KillLossTween(i);
                    heart.enabled = true;
                    SetAlpha(heart, 1f);
                }
                else if (wasActive)
                {
                    StartLossBlink(i);
                }
                else
                {
                    KillLossTween(i);
                    heart.enabled = false;
                    SetAlpha(heart, 1f);
                }
            }
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide()
        {
            KillAllLossTweens();
            gameObject.SetActive(false);
        }

        private void StartLossBlink(int index)
        {
            KillLossTween(index);
            var heart = hearts[index];
            heart.enabled = true;
            SetAlpha(heart, 1f);

            var seq = DOTween.Sequence().SetLink(heart.gameObject);
            for (int i = 0; i < heartLossBlinkCount; i++)
            {
                seq.Append(heart.DOFade(0f, heartLossBlinkInterval));
                seq.Append(heart.DOFade(1f, heartLossBlinkInterval));
            }
            seq.OnComplete(() =>
            {
                heart.enabled = false;
                SetAlpha(heart, 1f);
                _lossTweens.Remove(index);
            });

            _lossTweens[index] = seq;
        }

        private void KillLossTween(int index)
        {
            if (_lossTweens.TryGetValue(index, out var tween))
            {
                tween?.Kill();
                _lossTweens.Remove(index);
            }
        }

        private void KillAllLossTweens()
        {
            foreach (var kv in _lossTweens)
                kv.Value?.Kill();
            _lossTweens.Clear();
        }

        private static void SetAlpha(Image image, float a)
        {
            var color = image.color;
            color.a = a;
            image.color = color;
        }

        private void OnDestroy()
        {
            KillAllLossTweens();
        }
    }
}
