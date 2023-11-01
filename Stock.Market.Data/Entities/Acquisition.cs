using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock.Market.Data.Entities
{
    [Table(nameof(Acquisition))]
    public class Acquisition
    {
        public Guid Id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public decimal OriginalUnitCost { get; set; }

        [Required]
        public int Quantity { get; set; }

        public DateTime Date { get; set; }

        public Acquisition() { }

        public Acquisition(string companyName, string symbol, string originalCost, int quantity)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            OriginalUnitCost = ParseOriginalCost(originalCost);
            Date = DateTime.UtcNow;
            Quantity = quantity;
        }

        private decimal ParseOriginalCost(string str)
        {
            decimal.TryParse(str.TrimStart('$'), out var decimalValue);
            return decimalValue;
        }
    }
}
