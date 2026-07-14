using UnityEngine;
using TMPro;
using SweetSweeps.Server.Data;
using SweetSweeps.Infrastructure.Formatting;

namespace SweetSweeps.Menu.UI
{
    public class BalanceView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private string labelPrefix = string.Empty;
        [SerializeField] private string emptyPlaceholder = "-";
        [SerializeField] private bool showCurrencyCode = true;

        public void Render(WalletData wallet)
        {
            if (wallet == null)
            {
                balanceText.text = emptyPlaceholder;
                return;
            }

            string amount = showCurrencyCode
                ? CurrencyFormatter.Format(wallet.totalBalance, wallet.currency, wallet.fractions)
                : CurrencyFormatter.FormatAmount(wallet.totalBalance, wallet.fractions);

            balanceText.text = labelPrefix + amount;
        }
    }
}
