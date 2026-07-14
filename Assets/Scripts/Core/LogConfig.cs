using UnityEngine;

namespace SweetSweeps.Core
{
    internal static class LogConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Production WebGL build: suppress Log/Warning in the browser console,
            // keep Error + Exception only. Editor and non-WebGL builds are unaffected.
            Debug.unityLogger.filterLogType = LogType.Error;
#endif
        }
    }
}
