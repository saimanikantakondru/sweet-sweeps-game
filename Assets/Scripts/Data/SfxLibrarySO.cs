using UnityEngine;
using Sirenix.OdinInspector;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "SfxLibrary", menuName = "SweetSweeps/Audio/Sfx Library")]
    public class SfxLibrarySO : ScriptableObject
    {
        [Title("Movement")]
        [SerializeField] private SoundDefinition jump;
        [SerializeField] private SoundDefinition jumpDouble;
        [SerializeField] private SoundDefinition walkGrass;
        [SerializeField] private SoundDefinition walkSnow;
        [SerializeField] private SoundDefinition walkLava;

        [Title("Collect")]
        [SerializeField] private SoundDefinition collectGold;
        [SerializeField] private SoundDefinition collectSweeps;
        [SerializeField] private SoundDefinition collectBitter;

        [Title("Damage")]
        [SerializeField] private SoundDefinition damageIce;
        [SerializeField] private SoundDefinition damageLava;

        [Title("Death")]
        [SerializeField] private SoundDefinition deathFall;
        [SerializeField] private SoundDefinition deathFreeze;
        [SerializeField] private SoundDefinition deathBurn;

        [Title("Calamity / World")]
        [SerializeField] private SoundDefinition ambFreeze;
        [SerializeField] private SoundDefinition ambFreezeLevelTwo;
        [SerializeField] private SoundDefinition ambLava;
        [SerializeField] private SoundDefinition ambLavaLevelTwo;
        [SerializeField] private SoundDefinition calamityEnd;

        public SoundDefinition Jump => jump;
        public SoundDefinition JumpDouble => jumpDouble;
        public SoundDefinition DeathFall => deathFall;
        public SoundDefinition DeathFreeze => deathFreeze;
        public SoundDefinition DeathBurn => deathBurn;
        public SoundDefinition CalamityEnd => calamityEnd;

        public SoundDefinition WalkFor(Surface surface) => surface switch
        {
            Surface.Snow => walkSnow,
            Surface.Lava => walkLava,
            _            => walkGrass
        };

        public SoundDefinition AmbientFor(Surface surface) => surface switch
        {
            Surface.Snow => ambFreeze,
            Surface.Lava => ambLava,
            _            => null
        };

        public SoundDefinition AmbientLevelTwoFor(Surface surface) => surface switch
        {
            Surface.Snow => ambFreezeLevelTwo,
            Surface.Lava => ambLavaLevelTwo,
            _            => null
        };

        public SoundDefinition DamageFor(Surface surface) => surface switch
        {
            Surface.Snow => damageIce,
            Surface.Lava => damageLava,
            _            => null
        };

        public SoundDefinition CollectFor(CollectibleType type) => type switch
        {
            CollectibleType.Coin      => collectGold,
            CollectibleType.SsCoin    => collectSweeps,
            CollectibleType.SourCandy => collectBitter,
            _                         => null
        };
    }
}
