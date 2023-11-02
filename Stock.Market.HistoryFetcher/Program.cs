using Stock.Market.Common.Services;
using Stock.Market.Common.Services.Interfaces;
using Stock.Market.Data;

namespace Stock.Market.HistoryFetcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<INasdaqService, NasdaqService>();
                    services.AddHostedService<Worker>();
                    services.AddDbContext<ApplicationDBContext>(ServiceLifetime.Singleton);
                })
                .Build();

            host.Run();
        }
    }
}