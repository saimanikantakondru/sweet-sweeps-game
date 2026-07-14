using UnityEngine;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Calamities
{
    public class CalamityVisualEffect : ICalamityEffect, ICalamityIntensityListener
    {
        private readonly CalamityVisualProfileSO _profile;
        private readonly ICalamityVisualPresenter _presenter;

        private bool _isActive;
        private CalamityLevel _level = CalamityLevel.None;

        public CalamityVisualEffect(CalamityVisualProfileSO profile, ICalamityVisualPresenter presenter)
        {
            _profile = profile;
            _presenter = presenter;
        }

        public void Apply(CalamityLevel level, float intensity)
        {
            if (level == CalamityLevel.None)
            {
                Remove();
                return;
            }

            if (_profile == null)
            {
                Debug.LogWarning("[CalamityVisualEffect] No VisualProfile assigned on the CalamityData — visuals skipped.");
                return;
            }

            if (_presenter == null)
            {
                Debug.LogWarning("[CalamityVisualEffect] No visual presenter — CalamityVisualController not linked in GameplayLifetimeScope. Visuals skipped.");
                return;
            }

            _isActive = true;
            _level = level;
            _presenter.ApplyVisual(_profile, level, intensity);
        }

        public void OnIntensity(float intensity)
        {
            if (!_isActive) return;
            _presenter.UpdateIntensity(intensity);
        }

        public void Remove()
        {
            if (!_isActive) return;

            _isActive = false;
            _level = CalamityLevel.None;
            _presenter?.ClearVisual(_profile);
        }
    }
}
