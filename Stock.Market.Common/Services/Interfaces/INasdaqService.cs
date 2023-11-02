using Stock.Market.Common.Models;

namespace Stock.Market.Common.Services.Interfaces
{
    public interface INasdaqService
    {
        Task<NasdaqData?> FetchNasdaqData(string symbol);
    }
}
