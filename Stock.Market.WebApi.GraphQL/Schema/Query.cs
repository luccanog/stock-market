using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.WebApi.GraphQL.Schema.Types;
using Stock.Market.WebApi.GraphQL.Services;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;

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
            var acquisitionsBySymbol = _context.Acquisitions.AsEnumerable().GroupBy(a=>a.Symbol);
            var stockDataType = new List<StockDataType>();

            foreach (var acquisitions in acquisitionsBySymbol)
            {
                var data = await _nasdaqService.FetchNasdaqData(acquisitions.Key);

                decimal variation = CalculateProfitLoss(acquisitions.ToList(), Acquisition.ParseCost(data.PrimaryData.LastSalePrice));

                stockDataType.Add(new StockDataType()
                {
                    Symbol = acquisitions.Key,
                    SharesHeld = acquisitions.Sum(a=>a.Quantity),
                    TotalValue = acquisitions.Sum(x=> x.OriginalUnitCost * x.Quantity),
                    Variation = variation, 
                    CurrentDayReferencePrices = new() //retrieve historical data from db
                });
            }
            
            return stockDataType;
           
        }

        private decimal CalculateProfitLoss(List<Acquisition> acquisitions, decimal currentStockPrice)
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

            return profitLossPercentage;
        }
    }
}
