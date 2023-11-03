using Stock.Market.Common;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Common.Models;
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

        /// <summary>
        /// Sends a <see cref="Event"/> through Kafka with <see cref="Event.EventType"/> as <see cref="EventType.Buy"/>
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<bool> BuyStockShares(string symbol, int quantity)
        {
            ValidateQuantityOrThrow(quantity);

            NasdaqData? data = await GetNasdaqDataOrThrow(symbol);

            _messagingService.Send(EventsTopic, new Event(EventType.Buy, data!.CompanyName, symbol, Utils.ParseCost(data.PrimaryData.LastSalePrice), quantity));

            return true;
        }

        /// <summary>
        /// Sends a <see cref="Event"/> through Kafka with <see cref="Event.EventType"/> as <see cref="EventType.Sell"/>
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<bool> SellStockShares(string symbol, int quantity)
        {
            ValidateQuantityOrThrow(quantity);

            NasdaqData? data = await GetNasdaqDataOrThrow(symbol);

            var currentSharesTotalAmount = _context.Shares.AsEnumerable()
                .Where(c => c.Symbol.Equals(symbol))
                .Sum(x => x.Quantity);

            if (quantity > currentSharesTotalAmount)
            {
                string errorMessage = "The quantity of shares you are trying to sell is greater than the amount of shares you have.";
                errorMessage += "Please, check if you are trying to sell the correct Symbol";
                throw new GraphQLException(new Error(errorMessage));
            }

            _messagingService.Send(EventsTopic, new Event(
                EventType.Sell,
                data!.CompanyName,
                symbol,
                Utils.ParseCost(data.PrimaryData.LastSalePrice),
                quantity));

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
