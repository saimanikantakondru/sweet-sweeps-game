using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Core.Services
{
    public class PurpleCoinValueService : IPurpleCoinValueService
    {
        private float[] _normalValues = Array.Empty<float>();
        private float[] _calamityValues = Array.Empty<float>();
        private int _normalCursor;
        private int _calamityCursor;

        public void SetNormalValues(float[] values)
        {
            _normalValues = values ?? Array.Empty<float>();
            _normalCursor = 0;
        }

        public void SetCalamityValues(float[] values)
        {
            _calamityValues = values ?? Array.Empty<float>();
            _calamityCursor = 0;
        }

        public bool TryGetNextValue(GamePhase phase, out float value)
        {
            value = 0f;

            if (phase == GamePhase.Calamity)
            {
                if (_calamityCursor >= _calamityValues.Length)
                {
                    Debug.LogWarning("[PurpleCoinValueService] Calamity values exhausted.");
                    return false;
                }

                value = _calamityValues[_calamityCursor++];
                return true;
            }

            if (_normalCursor >= _normalValues.Length)
            {
                Debug.LogWarning("[PurpleCoinValueService] Normal values exhausted.");
                return false;
            }

            value = _normalValues[_normalCursor++];
            return true;
        }

        public void Reset()
        {
            _normalCursor = 0;
            _calamityCursor = 0;
        }
    }
}
