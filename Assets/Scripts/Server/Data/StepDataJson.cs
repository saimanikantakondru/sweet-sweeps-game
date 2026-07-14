using System;
using SweetSweeps.Data;

namespace SweetSweeps.Server.Data
{
    [Serializable]
    public class PurpleCoinValues
    {
        public float[] normal;
    }

    [Serializable]
    public class SessionDataJson
    {
        public int totalSteps;
        public string gamePlayUid;
        public bool reconnected;
        public float calamitySurvivalMultiplier;
        public PurpleCoinValues purpleCoinValues;
    }

    [Serializable]
    public class StepDataJson
    {
        public int step;
        public string phase;
        public int gold_coins;
        public int sour_candies;
        public int spiked_candies;
        public int purple_coins;
        public int meteors;

        public StepData ToStepData() => new StepData
        {
            step = step,
            phase = phase,
            goldCoins = gold_coins,
            sourCandies = sour_candies,
            spikedCandies = spiked_candies,
            purpleCoins = purple_coins,
            meteors = meteors
        };
    }
}