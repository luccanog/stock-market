using System.Globalization;

namespace Stock.Market.Common
{
    public static class Utils
    {
        public static decimal ParseCost(string stringValue)
        {
            decimal.TryParse(stringValue.TrimStart('$'), CultureInfo.InvariantCulture, out var decimalValue);

            return decimalValue;
        }
    }
}
