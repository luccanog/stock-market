using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Market.HistoryFetcher.Services
{
    public class DataFetcherService
    {
        private const string SYMBOL_PLACEHOLDER = "SYMBOL";
        private const int TIMEOUT_THRESHOLD = 10;

        private readonly string _apiUrl;

        public DataFetcherService(IConfiguration configuration)
        {
            _apiUrl = configuration["NasdaqApiUrl"]!;
        }

    }
}
