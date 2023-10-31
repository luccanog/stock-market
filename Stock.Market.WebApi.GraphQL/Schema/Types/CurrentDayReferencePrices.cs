namespace Stock.Market.WebApi.GraphQL.Schema.Types
{
    public class CurrentDayReferencePrices
    {
        /// <summary>
        /// Lowest stock's quote in the current day.
        /// </summary>
        public decimal LowestPrice { get; set; }

        /// <summary>
        /// Highest stock's quote in the current day.
        /// </summary>
        public decimal HighestPrice { get; set; }

        /// <summary>
        /// Average stock's quote in the current day.
        /// </summary>
        public decimal AveragePrice { get; set; }
    }
}
