﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Stock.Market.Data.Entities
{
    [Table(nameof(StockHistory))]
    public class StockHistory
    {
        public Guid Id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime InsertDate { get; set; }

        public StockHistory(string companyName, string symbol, decimal price)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            Price = price;
            InsertDate = DateTime.UtcNow;
        }
    }
}
