using System.Globalization;

namespace Stock.Market.Common
{
    public static class Utils
    {
        public static decimal ParseCost(string stringValue)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");

            decimal.TryParse(stringValue.TrimStart('$'), culture, out var decimalValue);

            return decimalValue;
        }
    }
}
