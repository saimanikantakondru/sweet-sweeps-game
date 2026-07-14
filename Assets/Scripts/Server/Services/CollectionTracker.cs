using UnityEngine;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Data;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Server.Services
{
    public class CollectionTracker : ICollectionTracker
    {
        private int _goldCoins;
        private int _ssCoins;
        private int _ssCoinsNormal;
        private int _ssCoinsCalamity;
        private int _sourCandiesHit;
        private bool _spikedCandyHit;
        private bool _survived;
        private int _survivalTimeMs;

        public void TrackCoinCollected()
        {
            _goldCoins++;
        }

        public void TrackSsCoinCollected(GamePhase phase)
        {
            _ssCoins++;

            if (phase == GamePhase.Calamity)
                _ssCoinsCalamity++;
            else
                _ssCoinsNormal++;
        }

        public void TrackSourCandyHit()
        {
            _sourCandiesHit++;
        }

        public void TrackSpikedCandyHit()
        {
            _spikedCandyHit = true;
        }

        public void TrackSurvival(bool survived, int survivalTimeMs)
        {
            _survived = survived;
            _survivalTimeMs = survivalTimeMs;
        }

        public void Reset()
        {
            _goldCoins = 0;
            _ssCoins = 0;
            _ssCoinsNormal = 0;
            _ssCoinsCalamity = 0;
            _sourCandiesHit = 0;
            _spikedCandyHit = false;
            _survived = false;
            _survivalTimeMs = 0;
            Debug.Log("[CollectionTracker] Reset.");
        }

        public void SetDemo()
        {
            _goldCoins = 20;
            _ssCoins = 160;
            _ssCoinsNormal = 80;
            _ssCoinsCalamity = 80;
            _sourCandiesHit = 4;
            _spikedCandyHit = false;
            _survived = true;
            _survivalTimeMs = 29000;
            Debug.Log("[CollectionTracker] SetDemo.");
        }

        public CollectionReport Build() => new CollectionReport
        {
            goldCoinsCollected = _goldCoins,
            purpleCoinsCollected = _ssCoins,
            purpleCoinsCollectedNormal = _ssCoinsNormal,
            purpleCoinsCollectedCalamity = _ssCoinsCalamity,
            sourCandiesHit = _sourCandiesHit,
            spikedCandyHit = _spikedCandyHit,
            survived = _survived,
            survivalTime = _survivalTimeMs
        };
    }
}
