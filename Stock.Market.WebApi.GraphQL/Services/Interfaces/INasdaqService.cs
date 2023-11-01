using Stock.Market.WebApi.GraphQL.Models;

namespace Stock.Market.WebApi.GraphQL.Services.Interfaces
{
    public interface INasdaqService
    {
        Task<NasdaqData?> FetchNasdaqData(string symbol);
    }
}
