using System.Text.Json.Serialization;

namespace Stock.Market.Common.Models
{
    public class NasdaqResponse
    {
        public NasdaqData Data { get; set; }
    }

    public class NasdaqData
    {
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public PrimaryData PrimaryData { get; set; }

    }
    public class PrimaryData
    {
        public string LastSalePrice { get; set; }
    }
}
