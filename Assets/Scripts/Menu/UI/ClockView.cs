using System;
using UnityEngine;
using TMPro;

namespace SweetSweeps.Menu.UI
{
    public class ClockView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private string timeFormat = "HH:mm:ss";

        private float _accumulator;
        private string _lastRendered;

        private void OnEnable()
        {
            _accumulator = 0f;
            Render(DateTime.Now);
        }

        private void Update()
        {
            _accumulator += Time.unscaledDeltaTime;
            if (_accumulator < 0.25f) return;

            _accumulator = 0f;
            Render(DateTime.Now);
        }

        private void Render(DateTime now)
        {
            string formatted = now.ToString(timeFormat);
            if (formatted == _lastRendered) return;

            _lastRendered = formatted;
            timeText.text = formatted;
        }
    }
}
