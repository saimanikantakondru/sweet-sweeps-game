using UnityEngine;

namespace SweetSweeps.Data
{
    public readonly struct CoinRevealInfo
    {
        public readonly float Value;
        public readonly Vector3 WorldPosition;

        public CoinRevealInfo(float value, Vector3 worldPosition)
        {
            Value = value;
            WorldPosition = worldPosition;
        }
    }
}
