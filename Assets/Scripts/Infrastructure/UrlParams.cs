using System.Runtime.InteropServices;
using UnityEngine;

namespace SweetSweeps.Infrastructure
{
    public static class UrlParams
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetUrlParam(string paramName);
#endif

        public static string GetParam(string paramName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var value = GetUrlParam(paramName);
            return string.IsNullOrEmpty(value) ? null : value;
#else
            return null;
#endif
        }
    }
}