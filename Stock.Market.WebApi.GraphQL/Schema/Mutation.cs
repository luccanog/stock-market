﻿using Stock.Market.WebApi.GraphQL.Services.Interfaces;

namespace Stock.Market.WebApi.GraphQL.Schema
{
    public class Mutation
    {
        private readonly INasdaqService _nasdaqService;
        private readonly IMessagingService _messagingService;

        public Mutation(INasdaqService nasdaqService, IMessagingService messagingService)
        {
            _nasdaqService = nasdaqService;
            _messagingService = messagingService;
        }

        public async Task<bool> BuyStockShares(string symbol)
        {
            var data = await _nasdaqService.FetchNasdaqData(symbol);

            if (data is null)
            {
                throw new GraphQLException(new Error("Either the provied symbol does not exist or the Nasdaq API failed. Please check the provided data and try again."));
            }

            _messagingService.Send(data);

            return true;
        }
    }
}
