using UnityEngine;
using SweetSweeps.Gameplay.Level;
using Sirenix.OdinInspector;

namespace SweetSweeps.Data
{
    [CreateAssetMenu(fileName = "LevelPreset", menuName = "SweetSweeps/LevelPreset")]
    public class LevelPresetSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string worldId;

        [Header("Geometry")]
        [SerializeField, AssetsOnly, Tooltip("Prefab whose root carries a LevelContentRoot component.")]
        private LevelContentRoot levelContentPrefab;

        [Header("Calamity")]
        [SerializeField] private CalamityBinding calamityBinding;

        [Header("Audio")]
        [SerializeField, Tooltip("Gameplay music for this world. Falls back to AudioSettings.GameplayMusic when unset.")]
        private SoundDefinition gameplayMusic;

        [Header("Default Data References")]
        [SerializeField] private CollectibleDataSO coinData;
        [SerializeField] private CollectibleDataSO ssCoinData;
        [SerializeField] private CollectibleDataSO trapData;
        [SerializeField] private CollectibleDataSO sourCandyData;

        public string WorldId => worldId;
        public LevelContentRoot LevelContentPrefab => levelContentPrefab;
        public CalamityBinding CalamityBinding => calamityBinding;
        public SoundDefinition GameplayMusic => gameplayMusic;
        public CollectibleDataSO CoinData => coinData;
        public CollectibleDataSO SsCoinData => ssCoinData;
        public CollectibleDataSO TrapData => trapData;
        public CollectibleDataSO SourCandyData => sourCandyData;
    }
}