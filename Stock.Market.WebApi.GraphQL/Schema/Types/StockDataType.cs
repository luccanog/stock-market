namespace Stock.Market.WebApi.GraphQL.Schema.Types
{
    /// <summary>
    /// Info about acquired stocks.
    /// </summary>
    public class StockDataType
    {
        /// <summary>
        /// An acronym that represents a company in a stock market
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Profit/Loss is how much the stock price has changed since the user bought it.
        /// </summary>
        public decimal Variation { get; set; }

        /// <summary>
        /// How many shares you have of this stock.
        /// </summary>
        public int SharesHeld { get; set; }

        /// <summary>
        /// The total value of all your shares.
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Current day price information regarding this Stock.
        /// </summary>
        public CurrentDayReferencePrices CurrentDayReferencePrices { get; set; } = new();
    }
}

