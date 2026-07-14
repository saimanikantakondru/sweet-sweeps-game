using UnityEngine;
using SweetSweeps.Gameplay.Collectibles;

namespace SweetSweeps.Data
{
    public enum CollectibleType { Coin, SsCoin, SourCandy, Trap }

    [CreateAssetMenu(fileName = "CollectibleData", menuName = "SweetSweeps/CollectibleData")]
    public class CollectibleDataSO : ScriptableObject
    {
        [Header("Type")]
        [SerializeField] private CollectibleType type;

        [Header("Coin")]
        [SerializeField] private int scoreValue;

        [Header("Trap")]
        [SerializeField] private int damageValue;

        [Header("Visuals")]
        [SerializeField] private CollectibleView prefab;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float spawnOffsetY = 2f;

        public CollectibleType Type => type;
        public int ScoreValue => scoreValue;
        public int DamageValue => damageValue;
        public CollectibleView Prefab => prefab;
        public float Lifetime => lifetime;
        public float FadeDuration => fadeDuration;
        public float SpawnOffsetY => spawnOffsetY;
    }
}