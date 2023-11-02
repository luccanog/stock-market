using Stock.Market.Common;
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
            var sharesBySymbol = _context.Shares.AsEnumerable().GroupBy(a => a.Symbol);
            var stockDataType = new List<StockDataType>();

            foreach (var shares in sharesBySymbol)
            {
                var data = await _nasdaqService.FetchNasdaqData(shares.Key);

                string variation = CalculateProfitLoss(shares.ToList(), Utils.ParseCost(data!.PrimaryData.LastSalePrice));

                stockDataType.Add(new StockDataType()
                {
                    Symbol = shares.Key,
                    SharesHeld = shares.Sum(a => a.Quantity),
                    TotalValue = shares.Sum(x => x.OriginalUnitCost * x.Quantity),
                    Variation = variation,
                    CurrentDayReferencePrices = GetCurrentDayReferencePrices(shares)
                });
            }

            return stockDataType;

        }

        private CurrentDayReferencePrices GetCurrentDayReferencePrices(IGrouping<string, Shares> shares)
        {
            var sortedCurrentDayStocksPriceHistory = _context.StocksHistory
                .Where(s => DateAreEquals(s.InsertDate.Date, DateTime.UtcNow.Date))
                .Select(s => s.Price)
                .ToList();

            var heldSharesPrices = shares.Where(s => DateAreEquals(s.Date, DateTime.UtcNow.Date)).Select(s => s.OriginalUnitCost);
            sortedCurrentDayStocksPriceHistory.AddRange(heldSharesPrices);

            var allCurrentDayShares = sortedCurrentDayStocksPriceHistory.Distinct().Order();

            if (!allCurrentDayShares.Any())
            {
                return new CurrentDayReferencePrices();
            }

            var lowest = sortedCurrentDayStocksPriceHistory.First();
            var highest = sortedCurrentDayStocksPriceHistory.Last();
            var average = sortedCurrentDayStocksPriceHistory.Average();

            return new CurrentDayReferencePrices
            {
                AveragePrice = average,
                LowestPrice = lowest,
                HighestPrice = highest
            };


        }

        private bool DateAreEquals(DateTime date1, DateTime date2)
        {
            return date1.Year.Equals(date2.Year) && date1.Month.Equals(date2.Month) && date1.Day.Equals(date2.Day);
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
