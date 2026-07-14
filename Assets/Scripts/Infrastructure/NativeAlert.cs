using System.Runtime.InteropServices;
using UnityEngine;

namespace SweetSweeps.Infrastructure
{
    public static class NativeAlert
    {
        [DllImport("__Internal")]
        private static extern void ShowNativeAlert(string message);

        public static void Show(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowNativeAlert(message);
#else
            Debug.LogWarning($"[NativeAlert] {message}");
#endif
        }
    }
}