using System;

namespace SweetSweeps.Menu.Contracts
{
    public interface IMenuService
    {
        float   SelectedBet      { get; }
        float[] BetLadder        { get; }
        int       SelectedBetIndex { get; }
        bool      CanIncrement     { get; }
        bool      CanDecrement     { get; }
        bool      CanPlay          { get; }
        bool      HasAffordableBet { get; }

        event Action OnBetChanged;
        event Action OnAffordabilityChanged;

        bool IsAffordable(int index);

        void IncrementBet();
        void DecrementBet();
        void SelectMin();
        void SelectMax();
        void SelectByIndex(int index);
        void Play();
    }
}