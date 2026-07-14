using System;

namespace SweetSweeps.Server.Data
{
    [Serializable]
    public class WalletBalance
    {
        public float amount;
        public float onHold;
        public float total;
    }

    [Serializable]
    public class WalletData
    {
        public string currency;
        public WalletBalance cash;
        public WalletBalance bonus;
        public float totalBalance;
        public int fractions;
    }
}