using System;
using UnityEngine;

namespace SweetSweeps.Data
{
    public enum BiomeCalamityType
    {
        None,
        Frost,
        Heavy,
        Alternating
    }

    public enum EndingCalamityType
    {
        None,
        WorldDecay,
        Random
    }

    [Serializable]
    public class CalamityBinding
    {
        [SerializeField] private BiomeCalamityType biomeCalamity;
        [SerializeField] private EndingCalamityType endingCalamity = EndingCalamityType.WorldDecay;

        public BiomeCalamityType BiomeCalamity => biomeCalamity;
        public EndingCalamityType EndingCalamity => endingCalamity;

        public string BiomeCalamityId => biomeCalamity switch
        {
            BiomeCalamityType.Frost => "frost",
            BiomeCalamityType.Heavy => "heavy",
            BiomeCalamityType.Alternating => AlternatingResolver.Next(),
            _ => string.Empty
        };

        public string ResolveEndingCalamityId()
        {
            return endingCalamity switch
            {
                EndingCalamityType.WorldDecay => "world_decay",
                EndingCalamityType.Random => GetRandomEndingId(),
                _ => string.Empty
            };
        }

        private static string GetRandomEndingId()
        {
            string[] options = { "world_decay" };
            return options[UnityEngine.Random.Range(0, options.Length)];
        }
    }
}