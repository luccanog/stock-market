using Stock.Market.Data;

namespace Stock.Market.EventProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                    services.AddDbContext<ApplicationDBContext>();
                })
                .Build();

            host.Run();
        }
    }
}