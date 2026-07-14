using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Infrastructure.UI
{
    public class UiButtonClickBinder : MonoBehaviour
    {
        private readonly List<Button> _bound = new();

        private IUiAudioService _uiAudio;

        [Inject]
        public void Construct(IUiAudioService uiAudio)
        {
            _uiAudio = uiAudio;
        }

        private void Start()
        {
            var buttons = GetComponentsInChildren<Button>(true);

            foreach (var button in buttons)
            {
                button.onClick.AddListener(PlayClick);
                _bound.Add(button);
            }
        }

        private void OnDestroy()
        {
            foreach (var button in _bound)
                if (button != null)
                    button.onClick.RemoveListener(PlayClick);

            _bound.Clear();
        }

        private void PlayClick() => _uiAudio?.PlayClick();
    }
}
