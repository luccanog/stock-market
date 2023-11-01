﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Market.Data.Models
{
    public class Event
    {
        public Guid Id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public EventType EventType { get; set; }

        public Event()
        {
        }

        public Event(EventType eventType, string companyName, string symbol, decimal originalUnitCost, int quantity)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            Symbol = symbol;
            Value = originalUnitCost;
            Quantity = quantity;
            EventType = eventType;
        }
    }

    public enum EventType
    {
        Buy,
        Sell
    }
}
