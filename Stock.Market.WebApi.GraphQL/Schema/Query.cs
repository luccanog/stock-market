using Flurl.Util;
using Microsoft.Identity.Client;
using Stock.Market.Common;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.WebApi.GraphQL.Models;
using Stock.Market.WebApi.GraphQL.Schema.Types;
using System.ComponentModel.DataAnnotations;

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

        public StockPriceHistory GetPriceHistory(string symbol)
        {
            var sharesBySymbol = _context.StocksHistory.Where(a => a.Symbol.Equals(symbol)).AsEnumerable();

            if (!sharesBySymbol.Any())
            {
                throw new GraphQLException("No historical data found for this SYMBOL. Please check if the SYMBOL is corret. Bear in mind: the history is update each hour.");
            }

            var result = new StockPriceHistory(sharesBySymbol.First().CompanyName, sharesBySymbol.First().Symbol);

            foreach (var shares in sharesBySymbol.OrderByDescending(s => s.InsertDate))
            {
                result.Quotes.Add(new Quote(shares.InsertDate, shares.Price));
            }

            return result;
        }

        private CurrentDayReferencePrices GetCurrentDayReferencePrices(IGrouping<string, Shares> shares)
        {
            var sortedCurrentDayStocksPriceHistory = _context.StocksHistory
                .Where(s => s.InsertDate.Date.Equals(DateTime.UtcNow.Date))
                .Select(s => s.Price)
                .ToList();

            var heldSharesPrices = shares.Where(s => s.Date.Date.Equals(DateTime.UtcNow.Date)).Select(s => s.OriginalUnitCost);
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

            var roundedNumber = decimal.Round(profitLossPercentage, 2, MidpointRounding.AwayFromZero);

            return $"{roundedNumber.ToInvariantString()}%";
        }
    }

}
