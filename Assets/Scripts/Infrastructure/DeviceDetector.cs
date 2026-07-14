using System.Runtime.InteropServices;
using UnityEngine;

namespace SweetSweeps.Infrastructure
{
    public static class DeviceDetector
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool IsMobileBrowser();
#endif

        public static bool IsMobile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return IsMobileBrowser();
#else
            return SystemInfo.deviceType == DeviceType.Handheld
                   || UnityEngine.InputSystem.Touchscreen.current != null;
#endif
        }
    }
}