using SweetSweeps.Server.Data;
using SweetSweeps.Gameplay.Calamities;

namespace SweetSweeps.Server.Contracts
{
    public interface ICollectionTracker
    {
        void TrackCoinCollected();
        void TrackSsCoinCollected(GamePhase phase);
        void TrackSourCandyHit();
        void TrackSpikedCandyHit();
        void TrackSurvival(bool survived, int survivalTimeMs);
        void Reset();
        void SetDemo();
        CollectionReport Build();
    }
}