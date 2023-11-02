using Stock.Market.HistoryFetcher.Services.Interfaces;

namespace Stock.Market.HistoryFetcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDataFetcherService _dataFetcherService;
        private readonly TimeSpan _interval;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IDataFetcherService dataFetcherService)
        {
            _logger = logger;
            _interval = TimeSpan.Parse(configuration["FetchDataInterval"]!);
            _dataFetcherService = dataFetcherService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer timer = new PeriodicTimer(_interval);
            do
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _dataFetcherService.UpdateStocksHistory();

            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}