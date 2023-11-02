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
                    services.AddHostedService<Worker>();
                    services.AddDbContext<ApplicationDBContext>(ServiceLifetime.Singleton);
                })
                .Build();

            host.Run();
        }
    }
}