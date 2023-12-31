﻿using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Stock.Market.Common.Models;
using Stock.Market.Common.Services.Interfaces;
using System.Net;

namespace Stock.Market.Common.Services
{
    public class NasdaqService : INasdaqService
    {
        private const string SYMBOL_PLACEHOLDER = "SYMBOL";
        private const int TIMEOUT_THRESHOLD = 10;

        private readonly string _apiUrl;

        public NasdaqService(IConfiguration configuration)
        {
            _apiUrl = configuration["NasdaqApiUrl"]!;
        }


        public async Task<NasdaqData?> FetchNasdaqData(string symbol)
        {
            try
            {
                var requestUrl = _apiUrl.Replace(SYMBOL_PLACEHOLDER, symbol);

                var response = await requestUrl
                    .WithTimeout(TimeSpan.FromSeconds(TIMEOUT_THRESHOLD))
                    .WithCookie("Cookie", "dummy-cookie-for-testing")
                    .GetAsync();

                if (!response.StatusCode.Equals((int)HttpStatusCode.OK))
                {
                    return null;
                }

                var parsedResponse = await response.GetJsonAsync<NasdaqResponse>();
                return parsedResponse?.Data;
            }
            catch (FlurlHttpTimeoutException ex)
            {
                Console.WriteLine($"The Nasdaq API did not answered within the time limit: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}