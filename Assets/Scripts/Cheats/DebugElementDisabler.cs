using UnityEngine;

namespace SweetSweeps.Cheats
{
    public class DebugElementDisabler : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            enabled = false;
#else
            enabled = true;
#endif
        }
    }
}
