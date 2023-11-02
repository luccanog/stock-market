namespace Stock.Market.Common
{
    public static class Utils
    {
        public static decimal ParseCost(string stringValue)
        {
            decimal.TryParse(stringValue.TrimStart('$'), out var decimalValue);
            return decimalValue;
        }
    }
}
