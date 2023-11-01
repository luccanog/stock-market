using System.ComponentModel.DataAnnotations;

namespace Stock.Market.Data.Entities
{
    public class Share
    {

        public Guid Id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public decimal OriginalCost { get; set; }

        public DateTime AcquisitionDate { get; set; }

        public Share() { }

        public Share(string companyName, string symbol, string originalCost)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            OriginalCost = ParseOriginalCost(originalCost);
            AcquisitionDate = DateTime.UtcNow;
        }

        private decimal ParseOriginalCost(string str)
        {
            decimal.TryParse(str.TrimStart('$'), out var decimalValue);
            return decimalValue;
        }
    }
}
