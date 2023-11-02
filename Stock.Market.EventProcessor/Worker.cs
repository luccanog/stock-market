using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Stock.Market.Data;
using Stock.Market.Data.Entities;
using Stock.Market.Data.Models;
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
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                    if (eventMessage.EventType is EventType.Buy)
                    {
                        var shares = new Shares(eventMessage.CompanyName, eventMessage.Symbol, eventMessage.Value, eventMessage.Quantity);
                        context.Shares.Add(shares);
                    }
                    else
                    {
                        var totalSharesToSold = eventMessage.Quantity;

                        var shares = context.Shares.Where(s => s.Symbol == eventMessage.Symbol);

                        if (totalSharesToSold == shares.Sum(s => s.Quantity))
                        {
                            context.Shares.Where(s => s.Symbol == eventMessage.Symbol).ExecuteDelete();
                        }
                        else
                        {
                            var soldShares = 0;

                            foreach (var share in shares)
                            {
                                if (soldShares + share.Quantity <= totalSharesToSold)
                                {
                                    context.Shares.Remove(share);
                                    soldShares += share.Quantity;
                                }
                                else
                                {
                                    var remainingSharesToBeSold = totalSharesToSold - soldShares;
                                    share.Quantity = share.Quantity - remainingSharesToBeSold;
                                    soldShares += remainingSharesToBeSold;
                                }

                                if (soldShares == totalSharesToSold)
                                {
                                    break;
                                }
                            }

                        }
                    }

                    await context.SaveChangesAsync();
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