using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Gameplay.Calamities
{
    public class CalamityVisualController : MonoBehaviour, ICalamityVisualPresenter
    {
        private static readonly int DistortionStrengthId = Shader.PropertyToID("_DistortionStrength");
        private static readonly int EdgeFrostAmountId = Shader.PropertyToID("_EdgeFrostAmount");
        private static readonly int OverlayMaskId = Shader.PropertyToID("_OverlayMask");

        [Required]
        [SerializeField] private Volume volume;

        [SerializeField] private Material fullscreenMaterial;
        [SerializeField] private Image screenOverlay;

        [SerializeField, Min(0f)] private float blendSpeed = 4f;

        private CalamityVisualProfileSO _activeProfile;
        private CalamityLevel _activeLevel = CalamityLevel.None;

        private float _weight;
        private float _targetWeight;
        private float _overlayAlpha;
        private float _targetOverlayAlpha;

        private void Awake()
        {
            _weight = _targetWeight = 0f;
            _overlayAlpha = _targetOverlayAlpha = 0f;
            if (volume != null) volume.weight = 0f;
            SetOverlayAlpha(0f);
        }

        private void Update()
        {
            float t = blendSpeed <= 0f ? 1f : 1f - Mathf.Exp(-blendSpeed * Time.deltaTime);

            if (volume != null && !Mathf.Approximately(_weight, _targetWeight))
            {
                _weight = Approach(_weight, _targetWeight, t);
                volume.weight = _weight;
            }

            if (screenOverlay != null && !Mathf.Approximately(_overlayAlpha, _targetOverlayAlpha))
            {
                _overlayAlpha = Approach(_overlayAlpha, _targetOverlayAlpha, t);
                SetOverlayAlpha(_overlayAlpha);
            }
        }

        private static float Approach(float current, float target, float t)
        {
            float next = Mathf.Lerp(current, target, t);
            return Mathf.Abs(target - next) < 0.001f ? target : next;
        }

        public void ApplyVisual(CalamityVisualProfileSO profile, CalamityLevel level, float intensity)
        {
            if (profile == null) return;

            _activeProfile = profile;
            _activeLevel = level;

            if (volume != null && profile.VolumeProfile != null)
                volume.sharedProfile = profile.VolumeProfile;

            if (screenOverlay != null)
                screenOverlay.sprite = profile.ScreenOverlaySprite;

            PushState(intensity);
        }

        public void UpdateIntensity(float intensity)
        {
            if (_activeProfile == null) return;
            PushState(intensity);
        }

        public void ClearVisual(CalamityVisualProfileSO profile)
        {
            if (_activeProfile != profile) return;

            _activeProfile = null;
            _activeLevel = CalamityLevel.None;

            _targetWeight = 0f;
            _targetOverlayAlpha = 0f;
            ResetFullscreen();
        }

        private void PushState(float intensity)
        {
            _targetWeight = volume != null ? _activeProfile.EvaluateWeight(_activeLevel, intensity) : 0f;
            _targetOverlayAlpha = _activeProfile.EvaluateOverlayAlpha(_activeLevel, intensity);
            UpdateFullscreen(intensity);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (screenOverlay == null) return;
            Color c = screenOverlay.color;
            c.a = alpha;
            screenOverlay.color = c;
        }

        private void UpdateFullscreen(float intensity)
        {
            if (fullscreenMaterial == null) return;

            var settings = _activeProfile.GetLevel(_activeLevel);
            if (settings.UseFullscreenPass)
            {
                float curve = Mathf.Clamp01(_activeProfile.WeightByIntensity.Evaluate(intensity));
                fullscreenMaterial.SetFloat(DistortionStrengthId, settings.DistortionStrength * curve);
                fullscreenMaterial.SetFloat(EdgeFrostAmountId, settings.EdgeFrostAmount * curve);
                if (settings.OverlayMask != null)
                    fullscreenMaterial.SetTexture(OverlayMaskId, settings.OverlayMask);
            }
            else
            {
                ResetFullscreen();
            }
        }

        private void ResetFullscreen()
        {
            if (fullscreenMaterial == null) return;
            fullscreenMaterial.SetFloat(DistortionStrengthId, 0f);
            fullscreenMaterial.SetFloat(EdgeFrostAmountId, 0f);
        }
    }
}
