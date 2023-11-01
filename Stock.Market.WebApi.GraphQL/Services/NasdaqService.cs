using Confluent.Kafka;
using Stock.Market.WebApi.GraphQL.Models;
using Stock.Market.WebApi.GraphQL.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Stock.Market.WebApi.GraphQL.Services
{
    public class NasdaqService : INasdaqService
    {
        private const string SYMBOL_PLACEHOLDER = "SYMBOL";

        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public NasdaqService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient.DefaultRequestHeaders.Add("Cookie", "dummy-cookie-for-testing"); // Without the cookie the request will not work.

            _apiUrl = configuration["NasdaqApiUrl"]!;
        }

        public async Task<NasdaqData?> FetchNasdaqData(string symbol)
        {
            try
            {
                var requestUrl = _apiUrl.Replace(SYMBOL_PLACEHOLDER, symbol);

                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                var response = await _httpClient.SendAsync(request, cancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var parsedResponse = await response.Content.ReadFromJsonAsync<NasdaqResponse>();
                
                return parsedResponse?.NasdaqData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
