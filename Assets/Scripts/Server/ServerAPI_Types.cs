using System;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server
{
    [Serializable]
    public class StakeSettings
    {
        public float defaultStake;
        public float[] minMax;
        public float[] ladder;
    }

    [Serializable]
    public class GameSettings
    {
        public StakeSettings stakes;
    }

    [Serializable]
    public class UnfinishedGameData
    {
        public int levelPreset;
        public int calamityStartStep;
        public int totalSteps;
        public float totalWager;
        public string gamePlay;
        public string gameRound;
        public string status;
    }

    [Serializable]
    public class InitializeResponse
    {
        public GameSettings settings;
        public WalletData wallet;
        public WalletData[] wallets;
        public string[] currencies;
        public string session;
        public bool streamed;
        public UnfinishedGameData[] unfinishedGames;
    }
    
    [Serializable]
    public class PlayStartData
    {
        public int levelPreset;
        public int calamityStartStep;
        public int totalSteps;
        public float totalWager;
    }

    [Serializable]
    public class PlayStartResponse
    {
        public PlayStartData response;
        public string gamePlay;
        public string gameRound;
        public string gameRoundStatus;
        public WalletData wallet;
        public WalletData[] wallets;
    }

    [Serializable]
    public class GameActivityData
    {
        public string uid;
        public float credit;
        public float debit;
        public string action;
        public string status;
        public int goldCoinsCollected;
        public int purpleCoinsCollected;
        public float goldCoinScore;
        public bool survived;
        public int survivalTime;
        public string validationStatus;
    }

    [Serializable]
    public class PlayCompleteData
    {
        public GameActivityData[] gameActivities;
        public float totalWager;
        public float totalWin;
        public int goldCoinsCollected;
        public int purpleCoinsCollected;
        public float goldCoinScore;
        public bool survived;
        public int survivalTime;
        public float valuePerPurpleCoin;
        public bool jackpotAwarded;
    }

    [Serializable]
    public class PlayCompleteResponse
    {
        public PlayCompleteData response;
        public string gamePlay;
        public string gameRound;
        public string gameRoundStatus;
        public WalletData wallet;
        public WalletData[] wallets;
    }
}