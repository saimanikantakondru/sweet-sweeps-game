using System;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "CalamityVisualProfile", menuName = "SweetSweeps/Calamities/Visual Profile")]
    public class CalamityVisualProfileSO : ScriptableObject
    {
        [Serializable]
        public class LevelVisualSettings
        {
            [SerializeField, Range(0f, 1f)] private float maxVolumeWeight = 1f;
            [SerializeField, Range(0f, 1f)] private float screenOverlayAlpha;
            [SerializeField] private bool useFullscreenPass;
            [SerializeField, ShowIf(nameof(useFullscreenPass))] private float distortionStrength;
            [SerializeField, ShowIf(nameof(useFullscreenPass))] private float edgeFrostAmount;
            [SerializeField, ShowIf(nameof(useFullscreenPass))] private Texture overlayMask;

            public float MaxVolumeWeight => maxVolumeWeight;
            public float ScreenOverlayAlpha => screenOverlayAlpha;
            public bool UseFullscreenPass => useFullscreenPass;
            public float DistortionStrength => distortionStrength;
            public float EdgeFrostAmount => edgeFrostAmount;
            public Texture OverlayMask => overlayMask;
        }

        [BoxGroup("Post Process"), Required]
        [SerializeField] private VolumeProfile volumeProfile;

        [BoxGroup("Post Process")]
        [SerializeField] private AnimationCurve weightByIntensity =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [BoxGroup("Screen Overlay")]
        [SerializeField] private Sprite screenOverlaySprite;

        [BoxGroup("Levels"), HideLabel, Title("Level One")]
        [SerializeField] private LevelVisualSettings levelOne = new();

        [BoxGroup("Levels"), HideLabel, Title("Level Two")]
        [SerializeField] private LevelVisualSettings levelTwo = new();

        public VolumeProfile VolumeProfile => volumeProfile;
        public AnimationCurve WeightByIntensity => weightByIntensity;
        public Sprite ScreenOverlaySprite => screenOverlaySprite;

        public LevelVisualSettings GetLevel(CalamityLevel level)
        {
            return level == CalamityLevel.LevelTwo ? levelTwo : levelOne;
        }

        public float EvaluateWeight(CalamityLevel level, float intensity)
        {
            float curve = Mathf.Clamp01(weightByIntensity.Evaluate(intensity));
            return curve * GetLevel(level).MaxVolumeWeight;
        }

        public float EvaluateOverlayAlpha(CalamityLevel level, float intensity)
        {
            if (screenOverlaySprite == null) return 0f;
            float curve = Mathf.Clamp01(weightByIntensity.Evaluate(intensity));
            return curve * GetLevel(level).ScreenOverlayAlpha;
        }
    }
}
