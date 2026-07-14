using UnityEngine;

namespace SweetSweeps.Data
{
    public abstract class CalamityDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string calamityId;

        [Header("Visuals")]
        [SerializeField] private CalamityVisualProfileSO visualProfile;

        public string CalamityId => calamityId;
        public CalamityVisualProfileSO VisualProfile => visualProfile;

        // Movement Modifiers (default = no effect)
        public virtual float SpeedMultiplier => 1f;
        public virtual float AccelerationMultiplier => 1f;
        public virtual float DecelerationMultiplier => 1f;
        public virtual float DirectionChangeMultiplier => 1f;
        public virtual float JumpForceMultiplier => 1f;
    }
}