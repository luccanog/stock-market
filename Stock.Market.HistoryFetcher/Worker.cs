namespace Stock.Market.HistoryFetcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _interval;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _interval = TimeSpan.Parse(configuration["FetchDataInterval"]!);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_interval);
            do
            {

            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}