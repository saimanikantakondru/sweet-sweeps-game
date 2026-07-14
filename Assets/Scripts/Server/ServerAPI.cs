using System;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server
{
    [Serializable]
    public class InitializeRequest
    {
        public string token;
        public string brand = "sweetsweeps-dev";
        public string game = "coin-calamity";
    }

    [Serializable]
    public class InitializeRequestWithCurrency
    {
        public string token;
        public string brand = "sweetsweeps-dev";
        public string game = "coin-calamity";
        public string currency;
    }

    [Serializable]
    public class PlayStartRequest
    {
        public string token;
        public string game = "coin-calamity";
        public string action = "start";
        public float wager;
    }

    [Serializable]
    public class PlayCompleteRequest
    {
        public string token;
        public string game = "coin-calamity";
        public string gameRound;
        public string action = "complete";
        public CollectionReport data;
    }
}