using Confluent.Kafka;
using Stock.Market.Data.Entities;
using System.Text.Json;

namespace Stock.Market.EventProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _serviceProvider;

        private const string BuySharesTopic = "buy-shares-topic";

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(BuySharesTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var consumeResult = _consumer.Consume(stoppingToken);
                var command = JsonSerializer.Deserialize<Share>(consumeResult.Message.Value);

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    //IStorage<Note> storage =
                    //    scope.ServiceProvider.GetRequiredService<IStorage<Note>>();

                    //storage.Add(note);
                }
            }
            _consumer.Close();
        }

        ~Worker()
        {
            _consumer.Dispose();
        }
    }
}