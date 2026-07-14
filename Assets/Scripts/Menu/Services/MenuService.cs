using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SweetSweeps.Server.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Menu.Contracts;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Menu.Services
{
    public class MenuService : IMenuService, IDisposable
    {
        private readonly ISceneService _sceneService;
        private readonly IWorldSelectService _worldSelectService;
        readonly IGameSessionService _sessionService;

        public float   SelectedBet => BetLadder[SelectedBetIndex];
        public float[] BetLadder        { get; private set; } = Array.Empty<float>();
        public int       SelectedBetIndex { get; private set; }

        public bool CanIncrement =>
            SelectedBetIndex < BetLadder.Length - 1 &&
            BetLadder[SelectedBetIndex + 1] <= CurrentBalance;

        public bool CanDecrement => SelectedBetIndex > 0;

        public bool CanPlay =>
            BetLadder.Length > 0 &&
            SelectedBet <= CurrentBalance;

        public bool HasAffordableBet =>
            BetLadder.Length > 0 &&
            BetLadder[0] <= CurrentBalance;

        public event Action OnBetChanged;
        public event Action OnAffordabilityChanged;

        private float CurrentBalance => _sessionService.CurrentWallet?.totalBalance ?? 0f;

        public MenuService(
            ISceneService       sceneService,
            IWorldSelectService worldSelectService,
            IGameSessionService sessionService)
        {
            _sceneService       = sceneService;
            _worldSelectService = worldSelectService;
            _sessionService     = sessionService;

            _sessionService.OnWalletsChanged     += HandleWalletsChanged;
            _sessionService.OnSessionInitialized += HandleSessionInitialized;

            ApplyLadder();
        }

        public void IncrementBet()
        {
            if (!CanIncrement) return;
            SelectedBetIndex++;
            OnBetChanged?.Invoke();
        }

        public void DecrementBet()
        {
            if (!CanDecrement) return;
            SelectedBetIndex--;
            OnBetChanged?.Invoke();
        }

        public void SelectMin()
        {
            if (BetLadder.Length == 0 || SelectedBetIndex == 0) return;
            SelectedBetIndex = 0;
            OnBetChanged?.Invoke();
        }

        public void SelectMax()
        {
            if (BetLadder.Length == 0) return;

            int maxAffordable = Array.FindLastIndex(BetLadder, v => v <= CurrentBalance);
            if (maxAffordable < 0 || maxAffordable == SelectedBetIndex) return;

            SelectedBetIndex = maxAffordable;
            OnBetChanged?.Invoke();
        }

        public void SelectByIndex(int index)
        {
            if (BetLadder.Length == 0) return;
            if (index < 0 || index >= BetLadder.Length) return;
            if (index == SelectedBetIndex) return;
            if (!IsAffordable(index)) return;

            SelectedBetIndex = index;
            OnBetChanged?.Invoke();
        }

        public bool IsAffordable(int index)
        {
            if (index < 0 || index >= BetLadder.Length) return false;
            return BetLadder[index] <= CurrentBalance;
        }

        public void Play()
        {
            _sceneService.LoadGameplayAsync().Forget();
        }

        public void Dispose()
        {
            _sessionService.OnWalletsChanged     -= HandleWalletsChanged;
            _sessionService.OnSessionInitialized -= HandleSessionInitialized;
        }

        private void HandleWalletsChanged()
        {
            ClampIndexToAffordable();
            OnAffordabilityChanged?.Invoke();
        }

        private void HandleSessionInitialized()
        {
            ApplyLadder();
            OnBetChanged?.Invoke();
        }
        
        private void ApplyLadder()
        {
            var stakes = _sessionService.ServerGameSettings?.stakes;
            if (stakes == null)
            {
                Debug.LogWarning("[MenuService] No stakes data from server — using fallback.");
                BetLadder = new [] { 1f, 2f, 3f, 4f, 5f, 6f, 8f, 10f, 20f, 25f, 50f, 100f };
            }
            else
            {
                BetLadder = stakes.ladder;
            }
 
            float defaultStake = stakes?.defaultStake ?? BetLadder[0];
            int defaultIndex = Array.FindIndex(BetLadder, v => Mathf.Approximately(v, defaultStake));
            SelectedBetIndex = defaultIndex < 0 ? 0 : defaultIndex;

            ClampIndexToAffordable();

            Debug.Log($"[MenuService] Ladder loaded. Steps={BetLadder.Length} " +
                      $"Selected={SelectedBet} Balance={CurrentBalance}");
        }

        private void ClampIndexToAffordable()
        {
            if (BetLadder.Length == 0) return;

            int maxAffordable = Array.FindLastIndex(BetLadder, v => v <= CurrentBalance);
            if (maxAffordable < 0)
            {
                SelectedBetIndex = 0;
                return;
            }

            if (SelectedBetIndex > maxAffordable)
                SelectedBetIndex = maxAffordable;
        }
    }
}