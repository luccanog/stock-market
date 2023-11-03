namespace Stock.Market.WebApi.GraphQL.Models
{
    public class Quote
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }

        public Quote(DateTime date, decimal price)
        {
            Date = date;
            Price = price;
        }
    }
}
