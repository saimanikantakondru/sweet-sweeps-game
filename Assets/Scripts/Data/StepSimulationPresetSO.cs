using UnityEngine;

namespace SweetSweeps.Data
{
    public enum SimulationCalamityMode
    {
        None,
        Frost,
        Heavy,
        WorldDecay,
        Random
    }

    [CreateAssetMenu(fileName = "StepSimulationPreset", menuName = "SweetSweeps/StepSimulationPreset")]
    public class StepSimulationPresetSO : ScriptableObject
    {
        [Header("Session")]
        [SerializeField] private int totalSteps = 30;

        [Header("Calamity")]
        [SerializeField] private int calamityStartStep = 17;
        [SerializeField] private SimulationCalamityMode calamityMode = SimulationCalamityMode.Random;

        [Header("Collectibles per step")]
        [SerializeField] private Vector2Int goldCoinsRange = new Vector2Int(1, 3);
        [SerializeField] private Vector2Int ssCoinsRange = new Vector2Int(0, 2);
        [SerializeField] private Vector2Int sourCandiesRange = new Vector2Int(0, 2);

        public int TotalSteps => totalSteps;
        public int CalamityStartStep => calamityStartStep;
        public SimulationCalamityMode CalamityMode => calamityMode;
        public Vector2Int GoldCoinsRange => goldCoinsRange;
        public Vector2Int SsCoinsRange => ssCoinsRange;
        public Vector2Int SourCandiesRange => sourCandiesRange;

        public string ResolveCalamityType()
        {
            return calamityMode switch
            {
                SimulationCalamityMode.Frost => "frost",
                SimulationCalamityMode.Heavy => "heavy",
                SimulationCalamityMode.WorldDecay => "world_decay",
                SimulationCalamityMode.Random => GetRandom(),
                _ => string.Empty
            };
        }

        private static string GetRandom()
        {
            string[] types = { "frost", "heavy", "world_decay" };
            return types[UnityEngine.Random.Range(0, types.Length)];
        }
    }
}