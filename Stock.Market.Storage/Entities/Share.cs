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

        public Share(string companyName, string symbol, decimal originalCost)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            OriginalCost = originalCost;
        }
    }
}
