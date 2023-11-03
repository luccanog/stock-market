using System.ComponentModel.DataAnnotations;

namespace Stock.Market.WebApi.GraphQL.Models
{

    public class StockPriceHistory
    {
        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Symbol { get; set; }

        public List<Quote> Quotes { get; set; } = new();

        public StockPriceHistory(string companyName, string symbol)
        {
            CompanyName = companyName;
            Symbol = symbol;
        }
    }
}
