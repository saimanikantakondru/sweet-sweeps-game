using UnityEngine;

namespace SweetSweeps.Data
{
    public static class AlternatingResolver
    {
        private static readonly string[] Sequence = { "heavy", "frost" };
        private static int _index = 0;

        public static string Next()
        {
            string result = Sequence[_index % Sequence.Length];
            _index++;
            Debug.Log($"[AlternatingResolver] BiomeCalamity={result} (round {_index})");
            return result;
        }

        public static void Reset()
        {
            _index = 0;
        }
    }
}