using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock.Market.Data.Entities
{
    [Table(nameof(Shares))]
    public class Shares
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

        public Shares() { }

        public Shares(string companyName, string symbol, decimal originalCost, int quantity)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            OriginalUnitCost = originalCost;
            Date = DateTime.UtcNow;
            Quantity = quantity;
        }

    }
}
