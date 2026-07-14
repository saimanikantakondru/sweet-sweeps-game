using System;
using System.Collections.Generic;
using UnityEngine;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Services
{
    public class GameSessionService : IGameSessionService
    {
        private const string DefaultGame = "coin-calamity";

        public string Token { get; private set; }
        public string Game { get; private set; } = DefaultGame;
        public string SessionUid { get; private set; }
        public string GamePlayUid { get; private set; }
        public string GameRoundUid { get; private set; }
        public PlayCompleteData LastRoundResult { get; private set; }
        public PlayStartData CurrentRoundData { get; private set; }
        public UnfinishedGameData[] UnfinishedGames { get; private set; }
        public GameSettings ServerGameSettings { get; private set; }

        private readonly Dictionary<string, WalletData> _wallets = new();
        private WalletData _activeWallet;

        public WalletData CurrentWallet => _activeWallet;
        public string ActiveCurrency => _activeWallet?.currency;

        public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
        public bool HasUnfinishedGame => UnfinishedGames is { Length: > 0 };

        public event Action OnWalletsChanged;
        public event Action OnAuthenticated;
        public event Action OnSessionInitialized;
        public event Action<PlayCompleteData> OnRoundResultReceived;
        public event Action OnRoundResultFailed;
        
        public void SetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogWarning("[GameSessionService] Empty token rejected.");
                return;
            }

            Token = token;
            Debug.Log("[GameSessionService] Token set.");
            OnAuthenticated?.Invoke();
        }

        public void SetGame(string game)
        {
            if (!string.IsNullOrEmpty(game))
                Game = game;
        }

        public void ApplyInitializeResponse(InitializeResponse response)
        {
            SessionUid = response.session;
            UnfinishedGames = response.unfinishedGames;
            ServerGameSettings = response.settings;

            ApplyWallets(response.wallet, response.wallets);

            if (HasUnfinishedGame)
                ApplyUnfinishedGame(UnfinishedGames[0]);

            OnSessionInitialized?.Invoke();
            OnWalletsChanged?.Invoke();
            Debug.Log($"[GameSessionService] Initialized. Session={SessionUid} Currency={ActiveCurrency} Balance={CurrentWallet?.totalBalance}");
        }

        public WalletData GetWallet(string currency)
        {
            if (string.IsNullOrEmpty(currency))
                return null;

            return _wallets.TryGetValue(currency, out var wallet) ? wallet : null;
        }

        private void ApplyWallets(WalletData active, WalletData[] all)
        {
            _wallets.Clear();

            if (all is { Length: > 0 })
            {
                foreach (var wallet in all)
                {
                    if (wallet != null && !string.IsNullOrEmpty(wallet.currency))
                        _wallets[wallet.currency] = wallet;
                }
            }

            if (active != null && !string.IsNullOrEmpty(active.currency))
            {
                if (!_wallets.ContainsKey(active.currency))
                    _wallets[active.currency] = active;

                _activeWallet = _wallets[active.currency];
                return;
            }

            _activeWallet = FirstWalletOrNull();
        }

        private WalletData FirstWalletOrNull()
        {
            foreach (var wallet in _wallets.Values)
                return wallet;

            return null;
        }
        
        private void ApplyUnfinishedGame(UnfinishedGameData unfinished)
        {
            GamePlayUid  = unfinished.gamePlay;
            GameRoundUid = unfinished.gameRound;
            
            CurrentRoundData = new PlayStartData
            {
                levelPreset       = unfinished.levelPreset,
                calamityStartStep = unfinished.calamityStartStep,
                totalSteps        = unfinished.totalSteps,
                totalWager        = unfinished.totalWager,
            };
 
            Debug.Log($"[GameSessionService] Resumed unfinished game. " +
                      $"GamePlay={GamePlayUid} Steps={CurrentRoundData.totalSteps}");
        }

        public void ApplyStartResponse(PlayStartResponse response)
        {
            GamePlayUid = response.gamePlay;
            GameRoundUid = response.gameRound;
            CurrentRoundData = response.response;

            ApplyWallets(response.wallet, response.wallets);

            OnWalletsChanged?.Invoke();
            Debug.Log($"[GameSessionService] Round started. GamePlay={GamePlayUid} Steps={CurrentRoundData?.totalSteps}");
        }

        public void ApplyCompleteResponse(PlayCompleteResponse response)
        {
            ApplyWallets(response.wallet, response.wallets);
            OnWalletsChanged?.Invoke();
            var r = response.response;
            LastRoundResult = r;
            
            Debug.Log(
                $"[GameSessionService] === ROUND COMPLETE ===\n" +
                $"GamePlay: {response.gamePlay}\n" +
                $"GameRound: {response.gameRound}\n" +
                $"Status: {response.gameRoundStatus}\n\n" +

                $"--- TOTAL ---\n" +
                $"TotalWager: {r?.totalWager}\n" +
                $"TotalWin: {r?.totalWin}\n" +
                $"GoldCoins: {r?.goldCoinsCollected}\n" +
                $"PurpleCoins: {r?.purpleCoinsCollected}\n" +
                $"GoldCoinScore: {r?.goldCoinScore}\n" +
                $"Survived: {r?.survived}\n" +
                $"SurvivalTime: {r?.survivalTime}\n" +
                $"ValuePerPurpleCoin: {r?.valuePerPurpleCoin}\n\n" +

                $"--- WALLET ---\n" +
                $"Currency: {response.wallet?.currency}\n" +
                $"Cash: {response.wallet?.cash?.amount} (Total: {response.wallet?.cash?.total})\n" +
                $"Bonus: {response.wallet?.bonus?.amount} (Total: {response.wallet?.bonus?.total})\n" +
                $"TotalBalance: {response.wallet?.totalBalance}\n"
            );

            if (r?.gameActivities != null)
            {
                foreach (var a in r.gameActivities)
                {
                    Debug.Log(
                        $"[Activity] UID={a.uid} | Action={a.action} | Status={a.status}\n" +
                        $"Validation={a.validationStatus}" +
                        $"Credit={a.credit} Debit={a.debit}\n" +
                        $"GoldCoins={a.goldCoinsCollected} PurpleCoins={a.purpleCoinsCollected}\n" +
                        $"Score={a.goldCoinScore} Survived={a.survived} Time={a.survivalTime}\n"
                    );
                }
            }

            OnRoundResultReceived?.Invoke(r);
        }

        public void NotifyRoundResultFailed()
        {
            Debug.LogWarning("[GameSessionService] Round result failed.");
            OnRoundResultFailed?.Invoke();
        }

        public void Clear()
        {
            GamePlayUid = null;
            GameRoundUid = null;
            CurrentRoundData = null;
            LastRoundResult = null;
            Debug.Log("[GameSessionService] Round data cleared.");
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        public void DebugSetBalance(float balance)
        {
            var currency = ActiveCurrency ?? CurrencyCodes.Default;
            var wallet = GetWallet(currency);
            if (wallet == null)
            {
                wallet = new WalletData { currency = currency, fractions = 2 };
                _wallets[currency] = wallet;
            }

            wallet.totalBalance = balance;
            _activeWallet = wallet;
            OnWalletsChanged?.Invoke();
            Debug.Log($"[GameSessionService] (SIM) Balance overridden to {balance}.");
        }
#endif
    }
}