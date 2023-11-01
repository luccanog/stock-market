using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
using Stock.Market.WebApi.GraphQL.Models;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;

namespace Stock.Market.WebApi.GraphQL.Schema
{
    public class Mutation
    {
        private readonly INasdaqService _nasdaqService;
        private readonly IMessagingService _messagingService;
        private readonly ApplicationDBContext _context;

        private const string EventsTopic = "event-topic";

        public Mutation(INasdaqService nasdaqService, IMessagingService messagingService, ApplicationDBContext context)
        {
            _nasdaqService = nasdaqService;
            _messagingService = messagingService;
            _context = context;
        }

        public async Task<bool> BuyStockShares(string symbol, int quantity)
        {
            ValidateQuantityOrThrow(quantity);

            NasdaqData? data = await GetNasdaqDataOrThrow(symbol);

            _messagingService.Send(EventsTopic, new Event(EventType.Buy, data.CompanyName, symbol, Shares.ParseCost(data.PrimaryData.LastSalePrice), quantity));

            return true;
        }

        public async Task<bool> SellStockShares(string symbol, int quantity)
        {
            ValidateQuantityOrThrow(quantity);

            NasdaqData? data = await GetNasdaqDataOrThrow(symbol);

            var currentSharesTotalAmount = _context.Shares.AsEnumerable()
                                                          .Where(c => c.Symbol.Equals(symbol))
                                                          .Sum(x => x.Quantity);

            if (quantity > currentSharesTotalAmount)
            {
                throw new GraphQLException(new Error("The quantity of shares you are trying to sell is greater than the amount of shares you have. Please, check if you are trying to sell the correct Symbol"));
            }

            _messagingService.Send(EventsTopic, new Event(EventType.Sell, data.CompanyName, symbol, Shares.ParseCost(data.PrimaryData.LastSalePrice), quantity));

            return true;
        }

        private async Task<NasdaqData?> GetNasdaqDataOrThrow(string symbol)
        {
            var data = await _nasdaqService.FetchNasdaqData(symbol);

            if (data is null)
            {
                throw new GraphQLException(new Error("Either the provied symbol does not exist or the Nasdaq API failed. Please check the provided data and try again."));
            }

            return data;
        }

        private static void ValidateQuantityOrThrow(int quantity)
        {
            if (quantity <= 0)
            {
                throw new GraphQLException(new Error("The quantity amount must be greater than or equals to 1"));
            }
        }
    }
}
