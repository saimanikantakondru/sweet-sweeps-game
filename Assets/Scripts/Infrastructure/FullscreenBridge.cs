using System.Runtime.InteropServices;
using UnityEngine;

namespace SweetSweeps.Infrastructure
{
    public static class FullscreenBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void SS_RequestFullscreen();
        [DllImport("__Internal")] private static extern void SS_ExitFullscreen();
        [DllImport("__Internal")] private static extern int SS_IsFullscreen();
#endif

        public static bool IsFullscreen
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return SS_IsFullscreen() == 1;
#else
                return Screen.fullScreen;
#endif
            }
        }

        public static void Request()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SS_RequestFullscreen();
#else
            Screen.fullScreen = true;
#endif
        }

        public static void Exit()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SS_ExitFullscreen();
#else
            Screen.fullScreen = false;
#endif
        }
    }
}
