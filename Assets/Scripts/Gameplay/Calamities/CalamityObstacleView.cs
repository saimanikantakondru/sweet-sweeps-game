using UnityEngine;

namespace SweetSweeps.Gameplay.Calamities
{
    public class CalamityObstacleView : MonoBehaviour
    {
        [SerializeField] private int damageAmount = 1;
        public int DamageAmount => damageAmount;
    }
}