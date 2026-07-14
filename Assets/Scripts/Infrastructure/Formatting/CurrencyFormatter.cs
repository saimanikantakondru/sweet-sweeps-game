using System.Globalization;

namespace SweetSweeps.Infrastructure.Formatting
{
    public static class CurrencyFormatter
    {
        private const int MaxFractions = 8;

        public static string Format(float amount, string currency, int fractions)
        {
            int safeFractions = Clamp(fractions);
            string formatted = amount.ToString("N" + safeFractions, CultureInfo.InvariantCulture);

            return string.IsNullOrEmpty(currency)
                ? formatted
                : $"{formatted} {currency}";
        }

        public static string FormatAmount(float amount, int fractions)
        {
            int safeFractions = Clamp(fractions);
            return amount.ToString("N" + safeFractions, CultureInfo.InvariantCulture);
        }

        private static int Clamp(int fractions)
        {
            if (fractions < 0) return 0;
            return fractions > MaxFractions ? MaxFractions : fractions;
        }
    }
}
