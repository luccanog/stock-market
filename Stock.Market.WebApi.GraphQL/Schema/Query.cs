using Stock.Market.WebApi.GraphQL.Schema.Types;

namespace Stock.Market.WebApi.GraphQL.Schema
{
    public class Query
    {

        // 2. Get a list of the stocks you are holding
        public IEnumerable<StockDataType> GetStockData()
        {
            return new List<StockDataType>()
            {
                new StockDataType{
                    Symbol = "AAPL",
                    SharesHeld = 5,
                    TotalValue = 10,
                    Variation = 1.2m,
                    CurrentDayReferencePrices = new CurrentDayReferencePrices
                    {
                        LowestPrice = 1,
                        AveragePrice = 2,
                        HighestPrice = 3,
                    },
                },
                new StockDataType{
                    Symbol = "MSFT",
                    SharesHeld = 5,
                    TotalValue = 10,
                    Variation = 1.2m,
                    CurrentDayReferencePrices = new CurrentDayReferencePrices
                    {
                        LowestPrice = 1,
                        AveragePrice = 2,
                        HighestPrice = 3,
                    },
                }
            };
        }
    }
}
