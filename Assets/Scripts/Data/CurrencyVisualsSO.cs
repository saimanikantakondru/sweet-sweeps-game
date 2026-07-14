using UnityEngine;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "CurrencyVisuals", menuName = "SweetSweeps/CurrencyVisuals")]
    public class CurrencyVisualsSO : ScriptableObject
    {
        [System.Serializable]
        public class CurrencyVisual
        {
            [SerializeField] private string currency;
            [SerializeField] private Sprite icon;

            public string Currency => currency;
            public Sprite Icon => icon;
        }

        [SerializeField] private CurrencyVisual[] visuals;

        public Sprite GetIcon(string currency)
        {
            if (string.IsNullOrEmpty(currency) || visuals == null)
                return null;

            foreach (var visual in visuals)
            {
                if (visual != null && visual.Currency == currency)
                    return visual.Icon;
            }

            return null;
        }
    }
}
