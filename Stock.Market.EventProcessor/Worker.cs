using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
using Stock.Market.EventProcessor.Service.Interfaces;
using System.Text.Json;

namespace Stock.Market.EventProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _serviceProvider;

        private const string BuySharesTopic = "event-topic";

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConsumer<Ignore, string> consumer)
        {
            _logger = logger;
            _consumer = consumer;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(BuySharesTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var consumeResult = _consumer.Consume(stoppingToken);
                var eventMessage = JsonSerializer.Deserialize<Event>(consumeResult.Message.Value);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    await eventService.Handle(eventMessage!);
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