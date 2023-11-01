using System.Text.Json.Serialization;

namespace Stock.Market.WebApi.GraphQL.Models
{
    public class NasdaqResponse
    {
        [JsonPropertyName("Data")]
        public NasdaqData NasdaqData { get; set; }
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
