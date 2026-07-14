using System;

namespace SweetSweeps.Server.Data
{
    [Serializable]
    public class CollectionReport
    {
        public int goldCoinsCollected;
        public int purpleCoinsCollected;
        public int purpleCoinsCollectedNormal;
        public int purpleCoinsCollectedCalamity;
        public int sourCandiesHit;
        public bool spikedCandyHit;
        public bool survived;
        public int survivalTime;
    }
}