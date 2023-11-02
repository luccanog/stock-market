using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.HistoryFetcher.Services.Interfaces;

namespace Stock.Market.HistoryFetcher.Services
{
    public class DataFetcherService : IDataFetcherService
    {
        private readonly INasdaqService _nasdaqService;
        private readonly ApplicationDBContext _context;

        public DataFetcherService(INasdaqService nasdaqService, ApplicationDBContext context)
        {
            _nasdaqService = nasdaqService;
            _context = context;
        }

        public async Task UpdateStocksHistory()
        {
            var symbols = _context.Shares.Select(s => s.Symbol).Distinct().ToList();

            foreach (var symbol in symbols)
            {
                var data = await _nasdaqService.FetchNasdaqData(symbol);
                var roster = new StockHistory(data.CompanyName, symbol, Shares.ParseCost(data.PrimaryData.LastSalePrice));
                _context.StocksHistory.Add(roster);
            }

            _context.SaveChanges();
        }

    }
}
