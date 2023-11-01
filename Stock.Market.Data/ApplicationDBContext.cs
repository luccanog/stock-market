using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stock.Market.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Stock.Market.Data
{
    [ExcludeFromCodeCoverage]
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options) { }

        protected ApplicationDBContext() { }

        public DbSet<Acquisition> Acquisitions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}