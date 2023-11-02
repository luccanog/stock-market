using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.WebApi.GraphQL.Schema.Types;

namespace Stock.Market.WebApi.GraphQL.Schema
{
    public class Query
    {
        private readonly ApplicationDBContext _context;
        private readonly INasdaqService _nasdaqService;

        public Query(ApplicationDBContext context, INasdaqService nasdaqService)
        {
            _context = context;
            _nasdaqService = nasdaqService;
        }

        /// <summary>
        /// Get a list of the stocks you are holding
        /// </summary>
        public async Task<IEnumerable<StockDataType>> GetStockData()
        {
            var sharesBySymbol = _context.Shares.AsEnumerable().GroupBy(a=>a.Symbol);
            var stockDataType = new List<StockDataType>();

            foreach (var shares in sharesBySymbol)
            {
                var data = await _nasdaqService.FetchNasdaqData(shares.Key);

                string variation = CalculateProfitLoss(shares.ToList(), Shares.ParseCost(data!.PrimaryData.LastSalePrice));

                stockDataType.Add(new StockDataType()
                {
                    Symbol = shares.Key,
                    SharesHeld = shares.Sum(a=>a.Quantity),
                    TotalValue = shares.Sum(x=> x.OriginalUnitCost * x.Quantity),
                    Variation = variation, 
                    CurrentDayReferencePrices = new() //retrieve historical data from db
                });
            }
            
            return stockDataType;
           
        }

        private string CalculateProfitLoss(List<Shares> acquisitions, decimal currentStockPrice)
        {
            decimal totalCost = 0;
            decimal currentValue = 0;

            foreach (var stock in acquisitions)
            {
                totalCost += stock.OriginalUnitCost * stock.Quantity;
                currentValue += currentStockPrice * stock.Quantity;
            }

            decimal profitLoss = currentValue - totalCost;
            decimal profitLossPercentage = (profitLoss / totalCost) * 100;

            var roundedNumber = decimal.Round(profitLossPercentage, MidpointRounding.AwayFromZero);

            return $"{roundedNumber}%";
        }
    }
}
