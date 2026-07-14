using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;

namespace SweetSweeps.Infrastructure.UI
{
    public class CountdownView : MonoBehaviour
    {
        [SerializeField, Required] private TextMeshProUGUI label;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void SetText(string value)
        {
            if (label != null)
                label.text = value;
        }
    }
}
