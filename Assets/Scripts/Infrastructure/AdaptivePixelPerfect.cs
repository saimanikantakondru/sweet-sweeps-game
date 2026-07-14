using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SweetSweeps.Infrastructure
{
    [RequireComponent(typeof(PixelPerfectCamera))]
    public class AdaptivePixelPerfect : MonoBehaviour
    {
        [SerializeField] int refWidth  = 1920;
        [SerializeField] int refHeight = 1080;
        [SerializeField] int basePPU   = 129;

        private PixelPerfectCamera _ppc;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _ppc = GetComponent<PixelPerfectCamera>();
            Apply();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
                Apply();
        }

        private void Apply()
        {
            _lastWidth  = Screen.width;
            _lastHeight = Screen.height;
            
            float scaleY = (float)Screen.height / refHeight;
            float scale = scaleY;
            int adaptedPPU = Mathf.Max(1, Mathf.FloorToInt(basePPU * scale));

            _ppc.assetsPPU = adaptedPPU;
            _ppc.refResolutionX = refWidth;
            _ppc.refResolutionY = refHeight;

            Debug.Log($"[AdaptivePixelPerfect] {Screen.width}*{Screen.height} scale={scale:F3} PPU={adaptedPPU}");
        }
    }
}